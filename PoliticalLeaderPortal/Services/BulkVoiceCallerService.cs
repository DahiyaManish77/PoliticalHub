using PoliticalLeaderPortal.Areas.Admin.ViewModels.VoiceAgent;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PoliticalLeaderPortal.Services
{
    public class BulkVoiceCallerService
    {
        private readonly string _connectionString;
        private readonly VoiceAgentService _voice = new VoiceAgentService();

        public BulkVoiceCallerService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public BulkVoiceCallerDashboardVM GetDashboard(string baseUrl)
        {
            EnsureSchema();
            var voice = _voice.GetDashboard(baseUrl);
            var model = new BulkVoiceCallerDashboardVM
            {
                IsConfigured = voice.IsConfigured,
                IsEnabled = voice.IsEnabled,
                ProviderName = voice.ProviderName,
                PhoneNumber = voice.PhoneNumber,
                ReadinessPercent = voice.ReadinessPercent,
                MissingConfiguration = voice.MissingConfiguration.ToList(),
                EligibleMembers = EligibleCount(),
                EligiblePeople = GetEligiblePeople()
            };

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT c.BulkVoiceCampaignId,c.CampaignName,c.MessageText,c.LanguageCode,c.Status,c.CreatedDate,
COUNT(q.BulkVoiceQueueId) TotalRecipients,
SUM(CASE WHEN q.Status='Pending' THEN 1 ELSE 0 END) PendingCount,
SUM(CASE WHEN q.Status IN ('Dialing','InProgress') THEN 1 ELSE 0 END) InProgressCount,
SUM(CASE WHEN q.Status='Completed' THEN 1 ELSE 0 END) CompletedCount,
SUM(CASE WHEN q.Status IN ('Failed','Busy','NoAnswer','Canceled') THEN 1 ELSE 0 END) FailedCount
FROM dbo.BulkVoiceCampaign c
LEFT JOIN dbo.BulkVoiceQueue q ON q.BulkVoiceCampaignId=c.BulkVoiceCampaignId
GROUP BY c.BulkVoiceCampaignId,c.CampaignName,c.MessageText,c.LanguageCode,c.Status,c.CreatedDate
ORDER BY c.BulkVoiceCampaignId DESC;", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) model.Campaigns.Add(MapCampaign(reader));
            }
            return model;
        }

        public int CreateCampaign(BulkVoiceCampaignVM model, IEnumerable<int> selectedPersonIds, int userId)
        {
            EnsureSchema();
            var selected = (selectedPersonIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (selected.Count == 0) throw new InvalidOperationException("Select at least one consented member.");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    int campaignId;
                    using (var command = new SqlCommand(@"
INSERT dbo.BulkVoiceCampaign(CampaignName,MessageText,LanguageCode,Status,CreatedBy,CreatedDate)
VALUES(@Name,@Message,@Language,'Draft',@UserId,GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Name", Clean(model.CampaignName));
                        command.Parameters.AddWithValue("@Message", Clean(model.MessageText));
                        command.Parameters.AddWithValue("@Language", String.IsNullOrWhiteSpace(model.LanguageCode) ? "hi-IN" : model.LanguageCode);
                        command.Parameters.AddWithValue("@UserId", userId);
                        campaignId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    using (var command = new SqlCommand(@"
CREATE TABLE #Selected(PersonId INT NOT NULL PRIMARY KEY);
INSERT #Selected(PersonId)
SELECT DISTINCT TRY_CONVERT(INT,value) FROM STRING_SPLIT(@SelectedIds,',') WHERE TRY_CONVERT(INT,value) IS NOT NULL;
;WITH Eligible AS
(
 SELECT p.PersonId,p.FullName,p.MobileNumber,
 ROW_NUMBER() OVER(PARTITION BY COALESCE(NULLIF(p.NormalizedMobile,''),p.MobileNumber) ORDER BY p.PersonId) rn
 FROM dbo.PersonMaster p INNER JOIN #Selected s ON s.PersonId=p.PersonId
 WHERE p.IsActive=1 AND p.IsDeleted=0 AND p.VoiceCallConsent=1 AND p.IsOptedOut=0
   AND p.ConsentDate IS NOT NULL AND NULLIF(LTRIM(RTRIM(p.MobileNumber)),'') IS NOT NULL
)
INSERT dbo.BulkVoiceQueue(BulkVoiceCampaignId,PersonId,MemberName,PhoneNumber,Status,AttemptCount,CreatedDate)
SELECT @CampaignId,PersonId,FullName,MobileNumber,'Pending',0,GETDATE()
FROM Eligible WHERE rn=1;", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@CampaignId", campaignId);
                        command.Parameters.AddWithValue("@SelectedIds", String.Join(",", selected));
                        int queued = command.ExecuteNonQuery();
                        if (queued <= 0) throw new InvalidOperationException("None of the selected people are currently eligible for voice calls.");
                    }
                    transaction.Commit();
                    return campaignId;
                }
            }
        }

        public void Start(int campaignId)
        {
            EnsureSchema();
            var setting = _voice.GetSetting();
            ValidateProvider(setting);
            Execute("UPDATE dbo.BulkVoiceCampaign SET Status='Running',StartedDate=COALESCE(StartedDate,GETDATE()),ModifiedDate=GETDATE() WHERE BulkVoiceCampaignId=@Id AND Status IN ('Draft','Paused','Running')",
                new SqlParameter("@Id", campaignId));
            StartNext(campaignId);
        }

        public void Pause(int campaignId)
        {
            Execute("UPDATE dbo.BulkVoiceCampaign SET Status='Paused',ModifiedDate=GETDATE() WHERE BulkVoiceCampaignId=@Id AND Status='Running'",
                new SqlParameter("@Id", campaignId));
        }

        public void Resume(int campaignId)
        {
            Execute("UPDATE dbo.BulkVoiceCampaign SET Status='Running',ModifiedDate=GETDATE() WHERE BulkVoiceCampaignId=@Id AND Status='Paused'",
                new SqlParameter("@Id", campaignId));
            StartNext(campaignId);
        }

        public void Stop(int campaignId)
        {
            Execute(@"UPDATE dbo.BulkVoiceCampaign SET Status='Stopped',CompletedDate=GETDATE(),ModifiedDate=GETDATE() WHERE BulkVoiceCampaignId=@Id;
UPDATE dbo.BulkVoiceQueue SET Status='Canceled',CompletedDate=GETDATE() WHERE BulkVoiceCampaignId=@Id AND Status='Pending';",
                new SqlParameter("@Id", campaignId));
        }

        public string GetAnswerXml(int campaignId)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT MessageText,LanguageCode FROM dbo.BulkVoiceCampaign WHERE BulkVoiceCampaignId=@Id", connection))
            {
                command.Parameters.AddWithValue("@Id", campaignId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return "<Response><Hangup/></Response>";
                    string text = SecurityElement(Convert.ToString(reader[0]));
                    string language = SecurityElement(Convert.ToString(reader[1]));
                    return "<Response><Say language=\"" + language + "\">" + text +
                           "</Say><Say language=\"hi-IN\">भविष्य की कॉल बंद करने के लिए कृपया कार्यालय से संपर्क करें। धन्यवाद।</Say></Response>";
                }
            }
        }

        public void ProcessStatus(string providerCallId, string providerStatus)
        {
            if (String.IsNullOrWhiteSpace(providerCallId)) return;
            string status = NormalizeStatus(providerStatus);
            int campaignId = 0;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
UPDATE dbo.BulkVoiceQueue SET Status=@Status,
AnsweredDate=CASE WHEN @Status='InProgress' AND AnsweredDate IS NULL THEN GETDATE() ELSE AnsweredDate END,
CompletedDate=CASE WHEN @Status IN ('Completed','Failed','Busy','NoAnswer','Canceled') THEN GETDATE() ELSE CompletedDate END,
ModifiedDate=GETDATE()
WHERE ProviderCallId=@ProviderId;
SELECT TOP 1 BulkVoiceCampaignId FROM dbo.BulkVoiceQueue WHERE ProviderCallId=@ProviderId;", connection))
            {
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@ProviderId", providerCallId);
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value) campaignId = Convert.ToInt32(result);
            }
            if (campaignId > 0 && IsTerminal(status))
            {
                CompleteIfFinished(campaignId);
                StartNext(campaignId);
            }
        }

        private void StartNext(int campaignId)
        {
            for (int failures = 0; failures < 5; failures++)
            {
                QueueTarget target = ClaimNext(campaignId);
                if (target == null) { CompleteIfFinished(campaignId); return; }
                try
                {
                    string providerId = PlaceCall(target);
                    Execute("UPDATE dbo.BulkVoiceQueue SET ProviderCallId=@ProviderId,Status='Dialing',ModifiedDate=GETDATE() WHERE BulkVoiceQueueId=@Id",
                        new SqlParameter("@ProviderId", providerId), new SqlParameter("@Id", target.QueueId));
                    return;
                }
                catch (Exception ex)
                {
                    Execute("UPDATE dbo.BulkVoiceQueue SET Status='Failed',LastError=@Error,CompletedDate=GETDATE(),ModifiedDate=GETDATE() WHERE BulkVoiceQueueId=@Id",
                        new SqlParameter("@Error", Truncate(ex.Message, 900)), new SqlParameter("@Id", target.QueueId));
                }
            }
        }

        private QueueTarget ClaimNext(int campaignId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SET XACT_ABORT ON; BEGIN TRAN;
IF NOT EXISTS(SELECT 1 FROM dbo.BulkVoiceCampaign WITH(UPDLOCK,HOLDLOCK) WHERE BulkVoiceCampaignId=@CampaignId AND Status='Running')
BEGIN ROLLBACK; RETURN; END;
IF EXISTS(SELECT 1 FROM dbo.BulkVoiceQueue WITH(UPDLOCK,HOLDLOCK) WHERE BulkVoiceCampaignId=@CampaignId AND Status IN ('Dialing','InProgress'))
BEGIN ROLLBACK; RETURN; END;
DECLARE @Id BIGINT=(SELECT TOP 1 BulkVoiceQueueId FROM dbo.BulkVoiceQueue WITH(UPDLOCK,READPAST)
 WHERE BulkVoiceCampaignId=@CampaignId AND Status='Pending' ORDER BY BulkVoiceQueueId);
IF @Id IS NULL BEGIN COMMIT; RETURN; END;
UPDATE dbo.BulkVoiceQueue SET Status='Dialing',AttemptCount=AttemptCount+1,LastAttemptDate=GETDATE(),ModifiedDate=GETDATE() WHERE BulkVoiceQueueId=@Id;
SELECT q.BulkVoiceQueueId,q.PhoneNumber,c.BulkVoiceCampaignId,c.MessageText,c.LanguageCode
FROM dbo.BulkVoiceQueue q JOIN dbo.BulkVoiceCampaign c ON c.BulkVoiceCampaignId=q.BulkVoiceCampaignId WHERE q.BulkVoiceQueueId=@Id;
COMMIT;", connection))
            {
                command.Parameters.AddWithValue("@CampaignId", campaignId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.Read() ? new QueueTarget
                    {
                        QueueId = Convert.ToInt64(reader[0]),
                        Phone = Convert.ToString(reader[1]),
                        CampaignId = Convert.ToInt32(reader[2])
                    } : null;
            }
        }

        private string PlaceCall(QueueTarget target)
        {
            var setting = _voice.GetSetting();
            ValidateProvider(setting);
            string baseUrl = setting.PublicBaseUrl.TrimEnd('/');
            string answer = baseUrl + "/VoiceAgentWebhook/BulkAnswer?key=" + HttpUtility.UrlEncode(setting.WebhookSecret) + "&campaignId=" + target.CampaignId;
            string status = baseUrl + "/VoiceAgentWebhook/Status?key=" + HttpUtility.UrlEncode(setting.WebhookSecret);
            string endpoint = "https://api.twilio.com/2010-04-01/Accounts/" + Uri.EscapeDataString(setting.AccountSid) + "/Calls.json";
            string body = "To=" + HttpUtility.UrlEncode(NormalizePhone(target.Phone)) +
                          "&From=" + HttpUtility.UrlEncode(setting.PhoneNumber) +
                          "&Url=" + HttpUtility.UrlEncode(answer) +
                          "&Method=POST&StatusCallback=" + HttpUtility.UrlEncode(status) +
                          "&StatusCallbackMethod=POST&StatusCallbackEvent=initiated&StatusCallbackEvent=answered&StatusCallbackEvent=completed";
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(setting.AccountSid, setting.AuthToken);
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string json = client.UploadString(endpoint, "POST", body);
                string marker = "\"sid\"";
                int at = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (at < 0) throw new InvalidOperationException("Provider did not return a call SID.");
                int colon = json.IndexOf(':', at + marker.Length);
                int first = json.IndexOf('"', colon + 1);
                int second = json.IndexOf('"', first + 1);
                if (first < 0 || second < 0) throw new InvalidOperationException("Invalid provider call response.");
                return json.Substring(first + 1, second - first - 1);
            }
        }

        private void ValidateProvider(VoiceAgentSettingVM setting)
        {
            if (!setting.IsEnabled) throw new InvalidOperationException("Enable the Voice Agent first.");
            if (String.IsNullOrWhiteSpace(setting.AccountSid) || String.IsNullOrWhiteSpace(setting.AuthToken) ||
                String.IsNullOrWhiteSpace(setting.PhoneNumber) || String.IsNullOrWhiteSpace(setting.PublicBaseUrl))
                throw new InvalidOperationException("Complete Voice Agent provider settings first.");
            if (!String.Equals(setting.ProviderName, "TwilioCompatible", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Live outbound calling is currently enabled for Twilio-compatible credentials only.");
        }

        private int EligibleCount()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"SELECT COUNT(DISTINCT COALESCE(NULLIF(NormalizedMobile,''),MobileNumber))
FROM dbo.PersonMaster WHERE IsActive=1 AND IsDeleted=0 AND VoiceCallConsent=1 AND IsOptedOut=0
AND ConsentDate IS NOT NULL AND NULLIF(LTRIM(RTRIM(MobileNumber)),'') IS NOT NULL;", connection))
            { connection.Open(); return Convert.ToInt32(command.ExecuteScalar()); }
        }

        private IList<BulkVoiceRecipientVM> GetEligiblePeople()
        {
            var list = new List<BulkVoiceRecipientVM>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
;WITH Eligible AS
(
 SELECT p.PersonId,p.FullName,p.MobileNumber,p.PreferredLanguage,p.ConsentDate,
        v.NameEnglish AS VillageName,gp.NameEnglish AS GramPanchayatName,
        ROW_NUMBER() OVER(PARTITION BY COALESCE(NULLIF(p.NormalizedMobile,''),p.MobileNumber) ORDER BY p.PersonId) rn
 FROM dbo.PersonMaster p
 LEFT JOIN dbo.VillageMaster v ON v.VillageId=p.VillageId
 LEFT JOIN dbo.GramPanchayatMaster gp ON gp.GramPanchayatId=p.GramPanchayatId
 WHERE p.IsActive=1 AND p.IsDeleted=0 AND p.VoiceCallConsent=1 AND p.IsOptedOut=0
   AND p.ConsentDate IS NOT NULL AND NULLIF(LTRIM(RTRIM(p.MobileNumber)),'') IS NOT NULL
)
SELECT PersonId,FullName,MobileNumber,PreferredLanguage,ConsentDate,VillageName,GramPanchayatName
FROM Eligible WHERE rn=1 ORDER BY FullName,PersonId;", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) list.Add(new BulkVoiceRecipientVM
                    {
                        PersonId = Convert.ToInt32(reader[0]),
                        FullName = Convert.ToString(reader[1]),
                        MobileNumber = Convert.ToString(reader[2]),
                        PreferredLanguage = Convert.ToString(reader[3]),
                        ConsentDate = reader[4] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader[4]),
                        VillageName = Convert.ToString(reader[5]),
                        GramPanchayatName = Convert.ToString(reader[6])
                    });
            }
            return list;
        }

        private void CompleteIfFinished(int campaignId)
        {
            Execute(@"IF NOT EXISTS(SELECT 1 FROM dbo.BulkVoiceQueue WHERE BulkVoiceCampaignId=@Id AND Status IN ('Pending','Dialing','InProgress'))
UPDATE dbo.BulkVoiceCampaign SET Status='Completed',CompletedDate=GETDATE(),ModifiedDate=GETDATE()
WHERE BulkVoiceCampaignId=@Id AND Status='Running';", new SqlParameter("@Id", campaignId));
        }

        private void Execute(string sql, params SqlParameter[] parameters)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            { if (parameters != null) command.Parameters.AddRange(parameters); connection.Open(); command.ExecuteNonQuery(); }
        }

        private void EnsureSchema()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
IF OBJECT_ID('dbo.BulkVoiceCampaign','U') IS NULL
CREATE TABLE dbo.BulkVoiceCampaign(
 BulkVoiceCampaignId INT IDENTITY PRIMARY KEY,CampaignName NVARCHAR(120) NOT NULL,MessageText NVARCHAR(1000) NOT NULL,
 LanguageCode NVARCHAR(10) NOT NULL DEFAULT('hi-IN'),Status NVARCHAR(20) NOT NULL DEFAULT('Draft'),
 StartedDate DATETIME NULL,CompletedDate DATETIME NULL,CreatedBy INT NULL,CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
 ModifiedDate DATETIME NULL);
IF OBJECT_ID('dbo.BulkVoiceQueue','U') IS NULL BEGIN
 CREATE TABLE dbo.BulkVoiceQueue(
  BulkVoiceQueueId BIGINT IDENTITY PRIMARY KEY,BulkVoiceCampaignId INT NOT NULL,PersonId INT NOT NULL,
  MemberName NVARCHAR(150) NULL,PhoneNumber NVARCHAR(30) NOT NULL,ProviderCallId NVARCHAR(150) NULL,
  Status NVARCHAR(20) NOT NULL DEFAULT('Pending'),AttemptCount INT NOT NULL DEFAULT(0),LastAttemptDate DATETIME NULL,
  AnsweredDate DATETIME NULL,CompletedDate DATETIME NULL,LastError NVARCHAR(1000) NULL,
  CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),ModifiedDate DATETIME NULL,
  CONSTRAINT FK_BulkVoiceQueue_Campaign FOREIGN KEY(BulkVoiceCampaignId) REFERENCES dbo.BulkVoiceCampaign(BulkVoiceCampaignId));
 CREATE INDEX IX_BulkVoiceQueue_Next ON dbo.BulkVoiceQueue(BulkVoiceCampaignId,Status,BulkVoiceQueueId);
 CREATE INDEX IX_BulkVoiceQueue_Provider ON dbo.BulkVoiceQueue(ProviderCallId);
END;", connection))
            { connection.Open(); command.ExecuteNonQuery(); }
        }

        private static BulkVoiceCampaignVM MapCampaign(IDataRecord r)
        {
            return new BulkVoiceCampaignVM
            {
                BulkVoiceCampaignId = Convert.ToInt32(r[0]), CampaignName = Convert.ToString(r[1]),
                MessageText = Convert.ToString(r[2]), LanguageCode = Convert.ToString(r[3]),
                Status = Convert.ToString(r[4]), CreatedDate = Convert.ToDateTime(r[5]),
                TotalRecipients = Number(r[6]), PendingCount = Number(r[7]), InProgressCount = Number(r[8]),
                CompletedCount = Number(r[9]), FailedCount = Number(r[10])
            };
        }
        private static int Number(object value) { return value == DBNull.Value ? 0 : Convert.ToInt32(value); }
        private static string Clean(string value) { return (value ?? "").Trim(); }
        private static string Truncate(string value, int size) { value = value ?? ""; return value.Length <= size ? value : value.Substring(0, size); }
        private static bool IsTerminal(string status) { return status == "Completed" || status == "Failed" || status == "Busy" || status == "NoAnswer" || status == "Canceled"; }
        private static string NormalizeStatus(string value)
        {
            value = (value ?? "").Trim().ToLowerInvariant();
            if (value == "completed") return "Completed";
            if (value == "answered" || value == "in-progress") return "InProgress";
            if (value == "busy") return "Busy";
            if (value == "no-answer" || value == "missed") return "NoAnswer";
            if (value == "canceled" || value == "cancelled") return "Canceled";
            if (value == "failed") return "Failed";
            return "Dialing";
        }
        private static string NormalizePhone(string value)
        {
            string digits = new String((value ?? "").Where(Char.IsDigit).ToArray());
            if (digits.Length == 10) return "+91" + digits;
            if (digits.Length == 12 && digits.StartsWith("91")) return "+" + digits;
            if ((value ?? "").Trim().StartsWith("+")) return "+" + digits;
            throw new InvalidOperationException("Invalid member phone number.");
        }
        private static string SecurityElement(string value)
        {
            return System.Security.SecurityElement.Escape(value ?? "");
        }
        private class QueueTarget { public long QueueId; public int CampaignId; public string Phone; }
    }
}
