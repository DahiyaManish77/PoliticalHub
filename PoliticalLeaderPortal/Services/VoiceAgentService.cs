using PoliticalLeaderPortal.Areas.Admin.ViewModels.VoiceAgent;
using PoliticalLeaderPortal.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;

namespace PoliticalLeaderPortal.Services
{
    public class VoiceAgentService
    {
        private readonly string _connectionString;
        public VoiceAgentService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public VoiceAgentDashboardVM GetDashboard(string baseUrl)
        {
            EnsureSchema();
            var setting = GetSetting();
            if (!String.IsNullOrWhiteSpace(setting.PublicBaseUrl)) baseUrl = setting.PublicBaseUrl;
            var model = new VoiceAgentDashboardVM
            {
                IsConfigured = !String.IsNullOrWhiteSpace(setting.AccountSid) && !String.IsNullOrWhiteSpace(setting.AuthToken)
                    && !String.IsNullOrWhiteSpace(setting.PhoneNumber) && !String.IsNullOrWhiteSpace(setting.PublicBaseUrl),
                IsEnabled = setting.IsEnabled,
                ProviderName = setting.ProviderName,
                PhoneNumber = setting.PhoneNumber,
                IncomingWebhookUrl = BuildWebhookUrl(baseUrl, setting.WebhookSecret, "Incoming"),
                StatusWebhookUrl = BuildWebhookUrl(baseUrl, setting.WebhookSecret, "Status"),
                RecordingWebhookUrl = BuildWebhookUrl(baseUrl, setting.WebhookSecret, "Recording"),
                HealthCheckUrl = BuildWebhookUrl(baseUrl, setting.WebhookSecret, "Health")
            };
            if (String.IsNullOrWhiteSpace(setting.AccountSid)) model.MissingConfiguration.Add("Provider account ID/API key");
            if (String.IsNullOrWhiteSpace(setting.AuthToken)) model.MissingConfiguration.Add("Provider auth token/API secret");
            if (String.IsNullOrWhiteSpace(setting.PhoneNumber)) model.MissingConfiguration.Add("Voice-enabled phone number");
            if (String.IsNullOrWhiteSpace(setting.PublicBaseUrl)) model.MissingConfiguration.Add("Public HTTPS base URL");
            if (!setting.IsEnabled) model.MissingConfiguration.Add("Enable live agent");
            model.ReadinessPercent = Math.Max(0, 100 - model.MissingConfiguration.Count * 20);
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"SELECT COUNT(1),
SUM(CASE WHEN Status IN ('answered','completed','in-progress') THEN 1 ELSE 0 END),
SUM(CASE WHEN Status IN ('missed','no-answer','busy','failed','canceled') THEN 1 ELSE 0 END),
SUM(CASE WHEN CAST(StartedAt AS date)=CAST(GETDATE() AS date) THEN 1 ELSE 0 END),
ISNULL(SUM(DurationSeconds),0)
FROM dbo.VoiceCallLog", connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.TotalCalls = Value(reader, 0);
                        model.AnsweredCalls = Value(reader, 1);
                        model.MissedCalls = Value(reader, 2);
                        model.CallsToday = Value(reader, 3);
                        model.TotalMinutes = (int)Math.Ceiling(Value(reader, 4) / 60m);
                    }
                }
                using (var command = new SqlCommand(CallSelect + " ORDER BY StartedAt DESC", connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read() && model.Calls.Count < 200) model.Calls.Add(MapCall(reader));
            }
            return model;
        }

        public VoiceAgentSettingVM GetSetting()
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"SELECT TOP 1 ProviderName,AccountSid,AuthToken,PhoneNumber,WebhookSecret,
PublicBaseUrl,IsEnabled,EnableRecording,RecordingConsentText,WelcomeMessageHindi,WelcomeMessageEnglish,
AiApiEndpoint,AiApiKey,RecordingRetentionDays FROM dbo.VoiceAgentSetting ORDER BY VoiceAgentSettingId", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return DefaultSetting();
                    return new VoiceAgentSettingVM
                    {
                        ProviderName = Convert.ToString(reader[0]),
                        AccountSid = Unprotect(Convert.ToString(reader[1])),
                        AuthToken = Unprotect(Convert.ToString(reader[2])),
                        PhoneNumber = Convert.ToString(reader[3]),
                        WebhookSecret = Convert.ToString(reader[4]),
                        PublicBaseUrl = Convert.ToString(reader[5]),
                        IsEnabled = Convert.ToBoolean(reader[6]),
                        EnableRecording = Convert.ToBoolean(reader[7]),
                        RecordingConsentText = Convert.ToString(reader[8]),
                        WelcomeMessageHindi = Convert.ToString(reader[9]),
                        WelcomeMessageEnglish = Convert.ToString(reader[10]),
                        AiApiEndpoint = Convert.ToString(reader[11]),
                        AiApiKey = Unprotect(Convert.ToString(reader[12])),
                        RecordingRetentionDays = Convert.ToInt32(reader[13])
                    };
                }
            }
        }

        public void SaveSetting(VoiceAgentSettingVM model, int userId)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.VoiceAgentSetting)
UPDATE dbo.VoiceAgentSetting SET ProviderName=@Provider,AccountSid=@Account,AuthToken=@Token,
PhoneNumber=@Phone,WebhookSecret=@Secret,PublicBaseUrl=@BaseUrl,IsEnabled=@Enabled,
EnableRecording=@Recording,RecordingConsentText=@Consent,WelcomeMessageHindi=@Hindi,
WelcomeMessageEnglish=@English,AiApiEndpoint=@AiEndpoint,AiApiKey=@AiKey,
RecordingRetentionDays=@Retention,ModifiedBy=@UserId,ModifiedDate=GETDATE()
ELSE INSERT dbo.VoiceAgentSetting(ProviderName,AccountSid,AuthToken,PhoneNumber,WebhookSecret,
PublicBaseUrl,IsEnabled,EnableRecording,RecordingConsentText,WelcomeMessageHindi,WelcomeMessageEnglish,
AiApiEndpoint,AiApiKey,RecordingRetentionDays,CreatedBy,CreatedDate)
VALUES(@Provider,@Account,@Token,@Phone,@Secret,@BaseUrl,@Enabled,@Recording,@Consent,@Hindi,@English,
@AiEndpoint,@AiKey,@Retention,@UserId,GETDATE())", connection))
            {
                command.Parameters.AddWithValue("@Provider", Clean(model.ProviderName));
                command.Parameters.AddWithValue("@Account", Protect(model.AccountSid));
                command.Parameters.AddWithValue("@Token", Protect(model.AuthToken));
                command.Parameters.AddWithValue("@Phone", Db(model.PhoneNumber));
                command.Parameters.AddWithValue("@Secret", Clean(model.WebhookSecret));
                command.Parameters.AddWithValue("@BaseUrl", Db(model.PublicBaseUrl));
                command.Parameters.AddWithValue("@Enabled", model.IsEnabled);
                command.Parameters.AddWithValue("@Recording", model.EnableRecording);
                command.Parameters.AddWithValue("@Consent", Clean(model.RecordingConsentText));
                command.Parameters.AddWithValue("@Hindi", Clean(model.WelcomeMessageHindi));
                command.Parameters.AddWithValue("@English", Clean(model.WelcomeMessageEnglish));
                command.Parameters.AddWithValue("@AiEndpoint", Db(model.AiApiEndpoint));
                command.Parameters.AddWithValue("@AiKey", Protect(model.AiApiKey));
                command.Parameters.AddWithValue("@Retention", model.RecordingRetentionDays);
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public long UpsertCall(string providerId, string direction, string from, string to, string status, int duration)
        {
            EnsureSchema();
            providerId = String.IsNullOrWhiteSpace(providerId) ? Guid.NewGuid().ToString("N") : providerId;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"DECLARE @Id BIGINT=(SELECT VoiceCallId FROM dbo.VoiceCallLog WHERE ProviderCallId=@ProviderId);
IF @Id IS NULL BEGIN
 INSERT dbo.VoiceCallLog(ProviderCallId,Direction,CallerNumber,CalledNumber,Status,StartedAt,DurationSeconds,CreatedDate)
 VALUES(@ProviderId,@Direction,@From,@To,@Status,GETDATE(),@Duration,GETDATE()); SET @Id=SCOPE_IDENTITY();
END ELSE UPDATE dbo.VoiceCallLog SET Status=@Status,DurationSeconds=CASE WHEN @Duration>0 THEN @Duration ELSE DurationSeconds END,
AnsweredAt=CASE WHEN @Status IN ('answered','in-progress') AND AnsweredAt IS NULL THEN GETDATE() ELSE AnsweredAt END,
EndedAt=CASE WHEN @Status IN ('completed','missed','no-answer','busy','failed','canceled') THEN GETDATE() ELSE EndedAt END,
ModifiedDate=GETDATE() WHERE VoiceCallId=@Id;
SELECT @Id;", connection))
            {
                command.Parameters.AddWithValue("@ProviderId", providerId);
                command.Parameters.AddWithValue("@Direction", Clean(direction));
                command.Parameters.AddWithValue("@From", Clean(from));
                command.Parameters.AddWithValue("@To", Clean(to));
                command.Parameters.AddWithValue("@Status", Clean(status).ToLowerInvariant());
                command.Parameters.AddWithValue("@Duration", Math.Max(0, duration));
                connection.Open();
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        public void AddSpeech(string providerId, string speech, string language, string intent)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"UPDATE dbo.VoiceCallLog SET Transcript=CASE WHEN NULLIF(Transcript,'') IS NULL THEN @Speech ELSE Transcript+CHAR(13)+@Speech END,
Language=@Language,Intent=@Intent,ModifiedDate=GETDATE() WHERE ProviderCallId=@ProviderId", connection))
            {
                command.Parameters.AddWithValue("@ProviderId", Clean(providerId));
                command.Parameters.AddWithValue("@Speech", Clean(speech));
                command.Parameters.AddWithValue("@Language", Clean(language));
                command.Parameters.AddWithValue("@Intent", Clean(intent));
                connection.Open(); command.ExecuteNonQuery();
            }
        }

        public void UpdateRecording(string providerId, string recordingUrl, int duration)
        {
            EnsureSchema();
            string localPath = TryDownloadRecording(recordingUrl);
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"UPDATE dbo.VoiceCallLog SET RecordingUrl=@Url,LocalRecordingPath=@Path,
RecordingDurationSeconds=@Duration,RecordingConsentGiven=1,ModifiedDate=GETDATE() WHERE ProviderCallId=@ProviderId", connection))
            {
                command.Parameters.AddWithValue("@ProviderId", Clean(providerId));
                command.Parameters.AddWithValue("@Url", Db(recordingUrl));
                command.Parameters.AddWithValue("@Path", Db(localPath));
                command.Parameters.AddWithValue("@Duration", Math.Max(0, duration));
                connection.Open(); command.ExecuteNonQuery();
            }
        }

        public VoiceCallVM GetCall(long id)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(CallSelect + " WHERE VoiceCallId=@Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id); connection.Open();
                using (var reader = command.ExecuteReader()) return reader.Read() ? MapCall(reader) : null;
            }
        }

        public bool IsValidSecret(string supplied)
        {
            var expected = GetSetting().WebhookSecret ?? "";
            if (String.IsNullOrWhiteSpace(supplied) || supplied.Length != expected.Length) return false;
            int difference = 0;
            for (int i = 0; i < supplied.Length; i++) difference |= supplied[i] ^ expected[i];
            return difference == 0;
        }

        public string BuildWebhookUrl(string baseUrl, string secret, string action)
        {
            baseUrl = String.IsNullOrWhiteSpace(baseUrl) ? "https://YOUR-DOMAIN.example" : baseUrl.TrimEnd('/');
            return baseUrl + "/VoiceAgentWebhook/" + action + "?key=" + HttpUtility.UrlEncode(secret);
        }

        public long CreateTestCall()
        {
            string id = "TEST-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            long callId = UpsertCall(id, "inbound", "+91-TEST-CALLER", GetSetting().PhoneNumber, "completed", 48);
            AddSpeech(id, "हमारे गांव की सड़क और नाली की समस्या दर्ज करनी है।", "hi-IN", "Roads and drainage");
            return callId;
        }

        private string TryDownloadRecording(string recordingUrl)
        {
            Uri uri;
            if (!Uri.TryCreate(recordingUrl, UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps) return null;
            try
            {
                var setting = GetSetting();
                string folder = HttpContext.Current.Server.MapPath("~/App_Data/VoiceRecordings/");
                Directory.CreateDirectory(folder);
                string fileName = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + ".mp3";
                using (var client = new WebClient())
                {
                    if (!String.IsNullOrWhiteSpace(setting.AccountSid))
                        client.Credentials = new NetworkCredential(setting.AccountSid, setting.AuthToken);
                    client.DownloadFile(uri, Path.Combine(folder, fileName));
                }
                return "App_Data/VoiceRecordings/" + fileName;
            }
            catch { return null; }
        }

        private static VoiceCallVM MapCall(IDataRecord r)
        {
            return new VoiceCallVM {
                VoiceCallId=Convert.ToInt64(r[0]),ProviderCallId=Convert.ToString(r[1]),Direction=Convert.ToString(r[2]),
                CallerNumber=Convert.ToString(r[3]),CalledNumber=Convert.ToString(r[4]),Status=Convert.ToString(r[5]),
                StartedAt=Convert.ToDateTime(r[6]),AnsweredAt=NullableDate(r[7]),EndedAt=NullableDate(r[8]),
                DurationSeconds=Value(r,9),Language=Convert.ToString(r[10]),Intent=Convert.ToString(r[11]),
                Transcript=Convert.ToString(r[12]),Summary=Convert.ToString(r[13]),RecordingUrl=Convert.ToString(r[14]),
                LocalRecordingPath=Convert.ToString(r[15])
            };
        }

        private const string CallSelect = @"SELECT VoiceCallId,ProviderCallId,Direction,CallerNumber,CalledNumber,Status,
StartedAt,AnsweredAt,EndedAt,DurationSeconds,Language,Intent,Transcript,Summary,RecordingUrl,LocalRecordingPath FROM dbo.VoiceCallLog";

        private void EnsureSchema()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
IF OBJECT_ID('dbo.VoiceAgentSetting','U') IS NULL
CREATE TABLE dbo.VoiceAgentSetting(VoiceAgentSettingId INT IDENTITY PRIMARY KEY,ProviderName NVARCHAR(50) NOT NULL,
AccountSid NVARCHAR(1000) NULL,AuthToken NVARCHAR(2000) NULL,PhoneNumber NVARCHAR(30) NULL,WebhookSecret NVARCHAR(100) NOT NULL,
PublicBaseUrl NVARCHAR(300) NULL,IsEnabled BIT NOT NULL DEFAULT(0),EnableRecording BIT NOT NULL DEFAULT(1),
RecordingConsentText NVARCHAR(500) NOT NULL,WelcomeMessageHindi NVARCHAR(500) NOT NULL,WelcomeMessageEnglish NVARCHAR(500) NOT NULL,
AiApiEndpoint NVARCHAR(300) NULL,AiApiKey NVARCHAR(2000) NULL,RecordingRetentionDays INT NOT NULL DEFAULT(90),
CreatedBy INT NULL,CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),ModifiedBy INT NULL,ModifiedDate DATETIME NULL);
IF OBJECT_ID('dbo.VoiceCallLog','U') IS NULL BEGIN
CREATE TABLE dbo.VoiceCallLog(VoiceCallId BIGINT IDENTITY PRIMARY KEY,ProviderCallId NVARCHAR(150) NOT NULL,Direction NVARCHAR(20) NOT NULL,
CallerNumber NVARCHAR(40) NULL,CalledNumber NVARCHAR(40) NULL,Status NVARCHAR(30) NOT NULL,StartedAt DATETIME NOT NULL,
AnsweredAt DATETIME NULL,EndedAt DATETIME NULL,DurationSeconds INT NOT NULL DEFAULT(0),Language NVARCHAR(20) NULL,Intent NVARCHAR(120) NULL,
Transcript NVARCHAR(MAX) NULL,Summary NVARCHAR(2000) NULL,RecordingUrl NVARCHAR(1000) NULL,LocalRecordingPath NVARCHAR(500) NULL,
RecordingDurationSeconds INT NOT NULL DEFAULT(0),RecordingConsentGiven BIT NOT NULL DEFAULT(0),CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
ModifiedDate DATETIME NULL); CREATE UNIQUE INDEX UX_VoiceCallLog_ProviderCallId ON dbo.VoiceCallLog(ProviderCallId); END;
IF NOT EXISTS(SELECT 1 FROM dbo.VoiceAgentSetting)
INSERT dbo.VoiceAgentSetting(ProviderName,WebhookSecret,IsEnabled,EnableRecording,RecordingConsentText,
WelcomeMessageHindi,WelcomeMessageEnglish,RecordingRetentionDays)
VALUES('TwilioCompatible',REPLACE(CONVERT(NVARCHAR(36),NEWID()),'-',''),0,1,
'यह कॉल सेवा, सुरक्षा और फॉलो-अप के लिए रिकॉर्ड की जा सकती है। जारी रखकर आप सहमति देते हैं।',
'नमस्कार। आप संगीत सोम जनसंपर्क कार्यालय के वॉइस सहायक से जुड़े हैं। कृपया अपनी समस्या बताइए।',
'Welcome to the Sangeet Som public-connect voice assistant. Please tell us how we can help you.',90);", connection))
            { connection.Open(); command.ExecuteNonQuery(); }
        }

        private static VoiceAgentSettingVM DefaultSetting() { return new VoiceAgentSettingVM { ProviderName="TwilioCompatible",WebhookSecret=Guid.NewGuid().ToString("N"),EnableRecording=true,RecordingRetentionDays=90 }; }
        private static object Db(string value) { return String.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim(); }
        private static string Clean(string value) { return (value ?? "").Trim(); }
        private static int Value(IDataRecord r, int i) { return r[i] == DBNull.Value ? 0 : Convert.ToInt32(r[i]); }
        private static DateTime? NullableDate(object value) { return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value); }
        private static string Protect(string value) { if (String.IsNullOrWhiteSpace(value)) return null; return Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(value), "VoiceAgent")); }
        private static string Unprotect(string value) { try { return String.IsNullOrWhiteSpace(value) ? null : Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(value), "VoiceAgent")); } catch { return null; } }
    }
}
