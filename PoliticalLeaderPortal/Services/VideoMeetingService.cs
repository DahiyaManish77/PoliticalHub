using PoliticalLeaderPortal.Areas.Admin.ViewModels.VideoMeeting;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PoliticalLeaderPortal.Services
{
    public class VideoMeetingService
    {
        private readonly string _connectionString;
        public VideoMeetingService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public VideoMeetingDashboardVM GetDashboard()
        {
            EnsureSchema();
            var model = new VideoMeetingDashboardVM { ProviderConnected = false };
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var counts = new SqlCommand(@"SELECT
SUM(CASE WHEN Status='Scheduled' AND ScheduledStart>=GETDATE() THEN 1 ELSE 0 END),
SUM(CASE WHEN Status='Live' THEN 1 ELSE 0 END),
SUM(CASE WHEN Status='Completed' THEN 1 ELSE 0 END)
FROM dbo.PortalVideoMeeting WHERE IsDeleted=0", connection))
                using (var reader = counts.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.UpcomingMeetings = Value(reader, 0);
                        model.LiveMeetings = Value(reader, 1);
                        model.CompletedMeetings = Value(reader, 2);
                    }
                }
                using (var command = new SqlCommand(@"SELECT TOP 100 VideoMeetingId,Title,MeetingType,ScheduledStart,
DurationMinutes,MaximumParticipants,Status,AllowRecording,SecureJoinToken
FROM dbo.PortalVideoMeeting WHERE IsDeleted=0 ORDER BY ScheduledStart DESC", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        model.Meetings.Add(new VideoMeetingListItemVM
                        {
                            VideoMeetingId = Convert.ToInt32(reader[0]),
                            Title = Convert.ToString(reader[1]),
                            MeetingType = Convert.ToString(reader[2]),
                            ScheduledStart = Convert.ToDateTime(reader[3]),
                            DurationMinutes = Convert.ToInt32(reader[4]),
                            MaximumParticipants = Convert.ToInt32(reader[5]),
                            Status = Convert.ToString(reader[6]),
                            AllowRecording = Convert.ToBoolean(reader[7]),
                            SecureJoinToken = Convert.ToString(reader[8])
                        });
                }
            }
            return model;
        }

        public int Save(VideoMeetingEditVM model, int userId)
        {
            EnsureSchema();
            if (model.AutoRecord && !model.AllowRecording)
                throw new InvalidOperationException("Automatic recording requires recording to be allowed.");
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
INSERT dbo.PortalVideoMeeting(Title,Description,MeetingType,ScheduledStart,DurationMinutes,MaximumParticipants,
AllowParticipantCamera,AllowParticipantMicrophone,AllowRecording,AutoRecord,RequireHostApproval,Invitees,
Status,SecureJoinToken,CreatedBy,CreatedDate)
OUTPUT INSERTED.VideoMeetingId
VALUES(@Title,@Description,@Type,@Start,@Duration,@Maximum,@Camera,@Microphone,@Recording,@AutoRecord,@Approval,
@Invitees,'Scheduled',@Token,@UserId,GETDATE())", connection))
            {
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 180).Value = model.Title.Trim();
                AddNullable(command, "@Description", 1000, model.Description);
                command.Parameters.Add("@Type", SqlDbType.NVarChar, 40).Value = model.MeetingType;
                command.Parameters.Add("@Start", SqlDbType.DateTime).Value = model.ScheduledStart;
                command.Parameters.Add("@Duration", SqlDbType.Int).Value = model.DurationMinutes;
                command.Parameters.Add("@Maximum", SqlDbType.Int).Value = model.MaximumParticipants;
                command.Parameters.Add("@Camera", SqlDbType.Bit).Value = model.AllowParticipantCamera;
                command.Parameters.Add("@Microphone", SqlDbType.Bit).Value = model.AllowParticipantMicrophone;
                command.Parameters.Add("@Recording", SqlDbType.Bit).Value = model.AllowRecording;
                command.Parameters.Add("@AutoRecord", SqlDbType.Bit).Value = model.AutoRecord;
                command.Parameters.Add("@Approval", SqlDbType.Bit).Value = model.RequireHostApproval;
                AddNullable(command, "@Invitees", 2000, model.Invitees);
                command.Parameters.Add("@Token", SqlDbType.NVarChar, 64).Value = Guid.NewGuid().ToString("N");
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool SetStatus(int id, string status, int userId)
        {
            if (status != "Live" && status != "Completed" && status != "Cancelled" && status != "Scheduled")
                throw new InvalidOperationException("Invalid meeting status.");
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"UPDATE dbo.PortalVideoMeeting SET Status=@Status,UpdatedBy=@UserId,
UpdatedDate=GETDATE() WHERE VideoMeetingId=@Id AND IsDeleted=0", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public void EnsureSchema()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
IF OBJECT_ID('dbo.PortalVideoMeeting','U') IS NULL
BEGIN
CREATE TABLE dbo.PortalVideoMeeting
(VideoMeetingId INT IDENTITY PRIMARY KEY,Title NVARCHAR(180) NOT NULL,Description NVARCHAR(1000) NULL,
MeetingType NVARCHAR(40) NOT NULL,ScheduledStart DATETIME NOT NULL,DurationMinutes INT NOT NULL,
MaximumParticipants INT NOT NULL,AllowParticipantCamera BIT NOT NULL,AllowParticipantMicrophone BIT NOT NULL,
AllowRecording BIT NOT NULL,AutoRecord BIT NOT NULL,RequireHostApproval BIT NOT NULL,Invitees NVARCHAR(2000) NULL,
Status NVARCHAR(20) NOT NULL,SecureJoinToken NVARCHAR(64) NOT NULL,ProviderMeetingId NVARCHAR(200) NULL,
IsDeleted BIT NOT NULL DEFAULT(0),CreatedBy INT NULL,CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
UpdatedBy INT NULL,UpdatedDate DATETIME NULL);
CREATE UNIQUE INDEX UX_PortalVideoMeeting_Token ON dbo.PortalVideoMeeting(SecureJoinToken);
END;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static int Value(IDataRecord reader, int index) { return reader[index] == DBNull.Value ? 0 : Convert.ToInt32(reader[index]); }
        private static void AddNullable(SqlCommand command, string name, int size, string value)
        {
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value =
                String.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
