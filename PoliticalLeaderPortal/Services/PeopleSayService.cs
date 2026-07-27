using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels.PeopleSay;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;

namespace PoliticalLeaderPortal.Services
{
    public sealed class PeopleSayDownload
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }

    public class PeopleSayService
    {
        private readonly string _connectionString;
        private const int MaximumVideoBytes = 100 * 1024 * 1024;

        public PeopleSayService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public PeopleSayHomeVM GetHome(int take = 20)
        {
            EnsureSchema();
            int limit = Math.Max(1, Math.Min(take, 30));
            var videos = GetYouTubePlaylistVideos(limit);
            foreach (var upload in GetVideos("Approved", null, limit))
            {
                if (videos.Count >= limit) break;
                videos.Add(upload);
            }
            return new PeopleSayHomeVM { Videos = videos };
        }

        private IList<PeopleSayVideoVM> GetYouTubePlaylistVideos(int take)
        {
            var videos = new List<PeopleSayVideoVM>();
            string apiKey = ConfigurationManager.AppSettings["YouTubeApiKey"];
            string playlistId = ConfigurationManager.AppSettings["PeopleSayYouTubePlaylistId"];
            if (String.IsNullOrWhiteSpace(apiKey) || String.IsNullOrWhiteSpace(playlistId)) return videos;
            try
            {
                string url = "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=" +
                    Uri.EscapeDataString(playlistId.Trim()) + "&maxResults=" + Math.Min(50, take) +
                    "&key=" + Uri.EscapeDataString(apiKey.Trim());
                using (var client = new WebClient())
                {
                    var root = JObject.Parse(client.DownloadString(url));
                    int syntheticId = -1;
                    foreach (var item in root["items"] ?? new JArray())
                    {
                        string videoId = Convert.ToString(item["snippet"]?["resourceId"]?["videoId"]);
                        if (String.IsNullOrWhiteSpace(videoId)) continue;
                        string thumbnail =
                            Convert.ToString(item["snippet"]?["thumbnails"]?["high"]?["url"]) ??
                            Convert.ToString(item["snippet"]?["thumbnails"]?["medium"]?["url"]) ??
                            "https://i.ytimg.com/vi/" + videoId + "/hqdefault.jpg";
                        DateTime published;
                        DateTime.TryParse(Convert.ToString(item["snippet"]?["publishedAt"]), out published);
                        videos.Add(new PeopleSayVideoVM
                        {
                            PeopleSayVideoId = syntheticId--,
                            PersonName = HttpUtility.HtmlDecode(Convert.ToString(item["snippet"]?["channelTitle"])) ?? "Community Voice",
                            AreaName = "YouTube",
                            Title = HttpUtility.HtmlDecode(Convert.ToString(item["snippet"]?["title"])) ?? "People Say About Som",
                            Message = HttpUtility.HtmlDecode(Convert.ToString(item["snippet"]?["description"])),
                            VideoPath = "https://www.youtube.com/embed/" + videoId + "?autoplay=1&rel=0",
                            YoutubeVideoId = videoId,
                            ThumbnailUrl = thumbnail,
                            IsYouTube = true,
                            Status = "Approved",
                            CreatedDate = published == default(DateTime) ? DateTime.Now : published
                        });
                    }
                }
            }
            catch
            {
                // Approved uploaded testimonials remain available if YouTube is temporarily unavailable.
            }
            return videos;
        }

        public int Submit(PeopleSaySubmissionVM model)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (!model.PublicationConsent) throw new InvalidOperationException("Publication consent is required.");
            EnsureSchema();
            using (var rateConnection = new SqlConnection(_connectionString))
            using (var rateCommand = new SqlCommand(@"
SELECT COUNT(1) FROM dbo.PeopleSayVideo
WHERE MobileNumber=@Mobile AND CreatedDate>=DATEADD(HOUR,-24,GETDATE())", rateConnection))
            {
                rateCommand.Parameters.Add("@Mobile", SqlDbType.NVarChar, 20).Value = Clean(model.MobileNumber);
                rateConnection.Open();
                if (Convert.ToInt32(rateCommand.ExecuteScalar()) >= 3)
                    throw new InvalidOperationException("A maximum of three video submissions is allowed per mobile number in 24 hours.");
            }
            string videoPath = SaveVideo(model.VideoFile, "people");
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
INSERT dbo.PeopleSayVideo
(PersonName,MobileNumber,AreaName,Title,Message,VideoPath,Status,PublicationConsent,CreatedDate)
OUTPUT INSERTED.PeopleSayVideoId
VALUES(@Name,@Mobile,@Area,@Title,@Message,@VideoPath,'Pending',1,GETDATE())", connection))
            {
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = Clean(model.PersonName);
                command.Parameters.Add("@Mobile", SqlDbType.NVarChar, 20).Value = Clean(model.MobileNumber);
                AddNullable(command, "@Area", 150, model.AreaName);
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 180).Value =
                    String.IsNullOrWhiteSpace(model.Title) ? "Message from " + Clean(model.PersonName) : Clean(model.Title);
                AddNullable(command, "@Message", 600, model.Message);
                command.Parameters.Add("@VideoPath", SqlDbType.NVarChar, 500).Value = videoPath;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public PeopleSayAdminVM GetAdmin(string status, string keyword)
        {
            EnsureSchema();
            var model = new PeopleSayAdminVM
            {
                Status = String.IsNullOrWhiteSpace(status) ? "Pending" : status,
                Keyword = keyword
            };
            model.Videos = GetVideos(model.Status, keyword, 300);
            model.Comments = GetComments("Pending", null, 100);
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT
SUM(CASE WHEN Status='Pending' THEN 1 ELSE 0 END),
SUM(CASE WHEN Status='Approved' THEN 1 ELSE 0 END),
SUM(CASE WHEN Status='Rejected' THEN 1 ELSE 0 END)
FROM dbo.PeopleSayVideo WHERE IsDeleted=0", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.PendingCount = IntValue(reader, 0);
                        model.ApprovedCount = IntValue(reader, 1);
                        model.RejectedCount = IntValue(reader, 2);
                    }
                }
            }
            return model;
        }

        public PeopleSayAnalyticsVM GetAnalytics()
        {
            EnsureSchema();
            var model = new PeopleSayAnalyticsVM();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT COUNT(1),
SUM(CASE WHEN Status='Approved' THEN 1 ELSE 0 END),
SUM(CASE WHEN Status='Pending' THEN 1 ELSE 0 END),
ISNULL(SUM(ViewCount),0),ISNULL(SUM(LikeCount),0),ISNULL(SUM(CommentCount),0),
ISNULL(SUM(ShareCount),0),ISNULL(SUM(DownloadCount),0)
FROM dbo.PeopleSayVideo WHERE IsDeleted=0", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.TotalSubmissions = IntValue(reader, 0);
                        model.ApprovedVideos = IntValue(reader, 1);
                        model.PendingVideos = IntValue(reader, 2);
                        model.TotalViews = IntValue(reader, 3);
                        model.TotalLikes = IntValue(reader, 4);
                        model.TotalComments = IntValue(reader, 5);
                        model.TotalShares = IntValue(reader, 6);
                        model.TotalDownloads = IntValue(reader, 7);
                    }
                }
            }
            model.TopVideos = GetVideos("Approved", null, 10)
                .OrderByDescending(x => x.LikeCount * 3 + x.CommentCount * 2 + x.ShareCount * 2 + x.ViewCount)
                .ToList();
            return model;
        }

        public bool SetStatus(int id, string status, string reason, int userId)
        {
            status = NormalizeStatus(status);
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
UPDATE dbo.PeopleSayVideo SET Status=@Status,RejectionReason=@Reason,
ApprovedBy=CASE WHEN @Status='Approved' THEN @UserId ELSE NULL END,
ApprovedOn=CASE WHEN @Status='Approved' THEN GETDATE() ELSE NULL END,UpdatedDate=GETDATE()
WHERE PeopleSayVideoId=@Id AND IsDeleted=0", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                AddNullable(command, "@Reason", 500, reason);
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public PeopleSayDownload ApproveDownloadAndDelete(int id, int userId)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    string title;
                    string virtualPath;
                    using (var find = new SqlCommand(@"
SELECT Title,VideoPath FROM dbo.PeopleSayVideo WITH (UPDLOCK,HOLDLOCK)
WHERE PeopleSayVideoId=@Id AND IsDeleted=0", connection, transaction))
                    {
                        find.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        using (var reader = find.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new InvalidOperationException("The video has already been processed or no longer exists.");
                            title = Convert.ToString(reader["Title"]);
                            virtualPath = Convert.ToString(reader["VideoPath"]);
                        }
                    }

                    string physicalPath = ResolvePhysicalPath(virtualPath);
                    if (String.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
                        throw new FileNotFoundException("The submitted video file could not be found.");

                    byte[] content = File.ReadAllBytes(physicalPath);
                    string extension = Path.GetExtension(physicalPath);

                    using (var delete = new SqlCommand(@"
DELETE FROM dbo.PeopleSayEngagement WHERE PeopleSayVideoId=@Id;
DELETE FROM dbo.PeopleSayComment WHERE PeopleSayVideoId=@Id;
DELETE FROM dbo.PeopleSayVideo WHERE PeopleSayVideoId=@Id;", connection, transaction))
                    {
                        delete.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        delete.ExecuteNonQuery();
                    }
                    transaction.Commit();

                    try { File.Delete(physicalPath); }
                    catch { /* The approved download remains valid; an orphan file can be cleaned later. */ }

                    return new PeopleSayDownload
                    {
                        Content = content,
                        ContentType = VideoMimeType(extension),
                        FileName = SafeFileName(title) + (String.IsNullOrWhiteSpace(extension) ? ".mp4" : extension)
                    };
                }
            }
        }

        public bool AddLeaderResponse(int id, HttpPostedFileBase file, string message)
        {
            string path = SaveVideo(file, "leader_response");
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
UPDATE dbo.PeopleSayVideo SET LeaderResponseVideoPath=@Path,LeaderResponseMessage=@Message,UpdatedDate=GETDATE()
WHERE PeopleSayVideoId=@Id AND IsDeleted=0 AND Status='Approved'", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                command.Parameters.Add("@Path", SqlDbType.NVarChar, 500).Value = path;
                AddNullable(command, "@Message", 600, message);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public PeopleSayVideoVM GetApprovedVideo(int id)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT PeopleSayVideoId,PersonName,AreaName,Title,Message,VideoPath,Status,RejectionReason,
LeaderResponseVideoPath,LeaderResponseMessage,ViewCount,LikeCount,CommentCount,ShareCount,DownloadCount,CreatedDate
FROM dbo.PeopleSayVideo
WHERE PeopleSayVideoId=@Id AND Status='Approved' AND IsDeleted=0", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.Read() ? MapVideo(reader) : null;
            }
        }

        public bool RecordEngagement(int id, string type, string visitorKey)
        {
            type = NormalizeEngagement(type);
            visitorKey = String.IsNullOrWhiteSpace(visitorKey) ? Guid.NewGuid().ToString("N") : visitorKey;
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (type == "Like")
                        {
                            using (var exists = new SqlCommand(@"SELECT COUNT(1) FROM dbo.PeopleSayEngagement
WHERE PeopleSayVideoId=@Id AND EngagementType='Like' AND VisitorKey=@Visitor", connection, transaction))
                            {
                                exists.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                                exists.Parameters.Add("@Visitor", SqlDbType.NVarChar, 100).Value = visitorKey;
                                if (Convert.ToInt32(exists.ExecuteScalar()) > 0) return false;
                            }
                        }
                        using (var insert = new SqlCommand(@"INSERT dbo.PeopleSayEngagement(PeopleSayVideoId,EngagementType,VisitorKey)
SELECT @Id,@Type,@Visitor WHERE EXISTS(SELECT 1 FROM dbo.PeopleSayVideo WHERE PeopleSayVideoId=@Id AND Status='Approved' AND IsDeleted=0)", connection, transaction))
                        {
                            insert.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                            insert.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = type;
                            insert.Parameters.Add("@Visitor", SqlDbType.NVarChar, 100).Value = visitorKey;
                            if (insert.ExecuteNonQuery() == 0) return false;
                        }
                        string column = type == "Like" ? "LikeCount" : type == "View" ? "ViewCount" : type == "Share" ? "ShareCount" : "DownloadCount";
                        using (var update = new SqlCommand("UPDATE dbo.PeopleSayVideo SET " + column + "=" + column + "+1 WHERE PeopleSayVideoId=@Id", connection, transaction))
                        {
                            update.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                            update.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool AddComment(PeopleSayCommentVM model)
        {
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
INSERT dbo.PeopleSayComment(PeopleSayVideoId,PersonName,CommentText,Status)
SELECT @Id,@Name,@Comment,'Pending'
WHERE EXISTS(SELECT 1 FROM dbo.PeopleSayVideo WHERE PeopleSayVideoId=@Id AND Status='Approved' AND IsDeleted=0)", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = model.PeopleSayVideoId;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = Clean(model.PersonName);
                command.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value = Clean(model.CommentText);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public IList<PeopleSayCommentVM> GetApprovedComments(int videoId)
        {
            return GetComments("Approved", videoId, 100);
        }

        public bool SetCommentStatus(int id, string status, int userId)
        {
            status = status == "Approved" ? "Approved" : "Rejected";
            EnsureSchema();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    int videoId;
                    using (var find = new SqlCommand("SELECT PeopleSayVideoId FROM dbo.PeopleSayComment WHERE PeopleSayCommentId=@Id AND IsDeleted=0", connection, transaction))
                    {
                        find.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        object result = find.ExecuteScalar();
                        if (result == null) return false;
                        videoId = Convert.ToInt32(result);
                    }
                    using (var update = new SqlCommand(@"UPDATE dbo.PeopleSayComment SET Status=@Status,ReviewedBy=@UserId,ReviewedOn=GETDATE() WHERE PeopleSayCommentId=@Id;
UPDATE dbo.PeopleSayVideo SET CommentCount=(SELECT COUNT(1) FROM dbo.PeopleSayComment WHERE PeopleSayVideoId=@VideoId AND Status='Approved' AND IsDeleted=0) WHERE PeopleSayVideoId=@VideoId;", connection, transaction))
                    {
                        update.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        update.Parameters.Add("@VideoId", SqlDbType.Int).Value = videoId;
                        update.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                        update.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        update.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return true;
                }
            }
        }

        private IList<PeopleSayVideoVM> GetVideos(string status, string keyword, int take)
        {
            EnsureSchema();
            var list = new List<PeopleSayVideoVM>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT TOP (@Take) PeopleSayVideoId,PersonName,AreaName,Title,Message,VideoPath,Status,RejectionReason,
LeaderResponseVideoPath,LeaderResponseMessage,ViewCount,LikeCount,CommentCount,ShareCount,DownloadCount,CreatedDate
FROM dbo.PeopleSayVideo WHERE IsDeleted=0
AND (@Status IS NULL OR Status=@Status)
AND (@Keyword IS NULL OR PersonName LIKE @Like OR Title LIKE @Like OR AreaName LIKE @Like)
ORDER BY CASE WHEN Status='Pending' THEN 0 ELSE 1 END, CreatedDate DESC", connection))
            {
                command.Parameters.Add("@Take", SqlDbType.Int).Value = take;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = String.IsNullOrWhiteSpace(status) || status == "All" ? (object)DBNull.Value : status;
                command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 180).Value = String.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword;
                command.Parameters.Add("@Like", SqlDbType.NVarChar, 190).Value = "%" + (keyword ?? String.Empty).Trim() + "%";
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) list.Add(MapVideo(reader));
            }
            return list;
        }

        private IList<PeopleSayCommentVM> GetComments(string status, int? videoId, int take)
        {
            EnsureSchema();
            var list = new List<PeopleSayCommentVM>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT TOP (@Take) PeopleSayCommentId,PeopleSayVideoId,PersonName,CommentText,Status,CreatedDate
FROM dbo.PeopleSayComment WHERE IsDeleted=0 AND Status=@Status
AND (@VideoId IS NULL OR PeopleSayVideoId=@VideoId) ORDER BY CreatedDate DESC", connection))
            {
                command.Parameters.Add("@Take", SqlDbType.Int).Value = take;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                command.Parameters.Add("@VideoId", SqlDbType.Int).Value = (object)videoId ?? DBNull.Value;
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        list.Add(new PeopleSayCommentVM
                        {
                            PeopleSayCommentId = Convert.ToInt32(reader[0]),
                            PeopleSayVideoId = Convert.ToInt32(reader[1]),
                            PersonName = Convert.ToString(reader[2]),
                            CommentText = Convert.ToString(reader[3]),
                            Status = Convert.ToString(reader[4]),
                            CreatedDate = Convert.ToDateTime(reader[5])
                        });
            }
            return list;
        }

        private static PeopleSayVideoVM MapVideo(IDataRecord reader)
        {
            return new PeopleSayVideoVM
            {
                PeopleSayVideoId = Convert.ToInt32(reader[0]),
                PersonName = Convert.ToString(reader[1]),
                AreaName = Convert.ToString(reader[2]),
                Title = Convert.ToString(reader[3]),
                Message = Convert.ToString(reader[4]),
                VideoPath = Convert.ToString(reader[5]),
                Status = Convert.ToString(reader[6]),
                RejectionReason = Convert.ToString(reader[7]),
                LeaderResponseVideoPath = Convert.ToString(reader[8]),
                LeaderResponseMessage = Convert.ToString(reader[9]),
                ViewCount = IntValue(reader, 10),
                LikeCount = IntValue(reader, 11),
                CommentCount = IntValue(reader, 12),
                ShareCount = IntValue(reader, 13),
                DownloadCount = IntValue(reader, 14),
                CreatedDate = Convert.ToDateTime(reader[15])
            };
        }

        private string SaveVideo(HttpPostedFileBase file, string prefix)
        {
            if (file == null || file.ContentLength <= 0) throw new InvalidOperationException("Please select a video.");
            if (file.ContentLength > MaximumVideoBytes) throw new InvalidOperationException("Video file must not exceed 100 MB.");
            string extension = (Path.GetExtension(file.FileName) ?? String.Empty).ToLowerInvariant();
            if (!new[] { ".mp4", ".webm", ".mov" }.Contains(extension))
                throw new InvalidOperationException("Only MP4, WEBM and MOV videos are allowed.");
            if (!HasValidVideoSignature(file, extension))
                throw new InvalidOperationException("The uploaded file is not a valid supported video.");
            string folder = HttpContext.Current.Server.MapPath("~/Uploads/PeopleSay/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = prefix + "_" + Guid.NewGuid().ToString("N") + extension;
            file.SaveAs(Path.Combine(folder, fileName));
            return "/Uploads/PeopleSay/" + fileName;
        }

        private static bool HasValidVideoSignature(HttpPostedFileBase file, string extension)
        {
            Stream stream = file.InputStream;
            long position = stream.CanSeek ? stream.Position : 0;
            var bytes = new byte[12];
            int read = stream.Read(bytes, 0, bytes.Length);
            if (stream.CanSeek) stream.Position = position;
            if (extension == ".webm")
                return read >= 4 && bytes[0] == 0x1A && bytes[1] == 0x45 && bytes[2] == 0xDF && bytes[3] == 0xA3;
            return read >= 8 && bytes[4] == 0x66 && bytes[5] == 0x74 && bytes[6] == 0x79 && bytes[7] == 0x70;
        }

        private void EnsureSchema()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
IF OBJECT_ID('dbo.PeopleSayVideo','U') IS NULL
BEGIN
CREATE TABLE dbo.PeopleSayVideo(PeopleSayVideoId INT IDENTITY PRIMARY KEY,PersonName NVARCHAR(150) NOT NULL,MobileNumber NVARCHAR(20) NOT NULL,AreaName NVARCHAR(150) NULL,Title NVARCHAR(180) NOT NULL,Message NVARCHAR(600) NULL,VideoPath NVARCHAR(500) NOT NULL,Status NVARCHAR(20) NOT NULL DEFAULT('Pending'),RejectionReason NVARCHAR(500) NULL,PublicationConsent BIT NOT NULL,ApprovedBy INT NULL,ApprovedOn DATETIME NULL,LeaderResponseVideoPath NVARCHAR(500) NULL,LeaderResponseMessage NVARCHAR(600) NULL,ViewCount INT NOT NULL DEFAULT(0),LikeCount INT NOT NULL DEFAULT(0),CommentCount INT NOT NULL DEFAULT(0),ShareCount INT NOT NULL DEFAULT(0),DownloadCount INT NOT NULL DEFAULT(0),IsDeleted BIT NOT NULL DEFAULT(0),CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),UpdatedDate DATETIME NULL);
END;
IF OBJECT_ID('dbo.PeopleSayComment','U') IS NULL
BEGIN
CREATE TABLE dbo.PeopleSayComment(PeopleSayCommentId INT IDENTITY PRIMARY KEY,PeopleSayVideoId INT NOT NULL,PersonName NVARCHAR(100) NOT NULL,CommentText NVARCHAR(500) NOT NULL,Status NVARCHAR(20) NOT NULL DEFAULT('Pending'),ReviewedBy INT NULL,ReviewedOn DATETIME NULL,IsDeleted BIT NOT NULL DEFAULT(0),CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),CONSTRAINT FK_PeopleSayComment_Video FOREIGN KEY(PeopleSayVideoId) REFERENCES dbo.PeopleSayVideo(PeopleSayVideoId));
END;
IF OBJECT_ID('dbo.PeopleSayEngagement','U') IS NULL
BEGIN
CREATE TABLE dbo.PeopleSayEngagement(PeopleSayEngagementId BIGINT IDENTITY PRIMARY KEY,PeopleSayVideoId INT NOT NULL,EngagementType NVARCHAR(20) NOT NULL,VisitorKey NVARCHAR(100) NOT NULL,CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),CONSTRAINT FK_PeopleSayEngagement_Video FOREIGN KEY(PeopleSayVideoId) REFERENCES dbo.PeopleSayVideo(PeopleSayVideoId));
CREATE UNIQUE INDEX UX_PeopleSayEngagement_UniqueLike ON dbo.PeopleSayEngagement(PeopleSayVideoId,EngagementType,VisitorKey) WHERE EngagementType='Like';
END;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string NormalizeStatus(string status)
        {
            if (status == "Approved" || status == "Rejected" || status == "Pending") return status;
            throw new InvalidOperationException("Invalid moderation status.");
        }
        private static string NormalizeEngagement(string type)
        {
            if (type == "Like" || type == "View" || type == "Share" || type == "Download") return type;
            throw new InvalidOperationException("Invalid engagement type.");
        }
        private static string Clean(string value) { return (value ?? String.Empty).Trim(); }
        private static int IntValue(IDataRecord record, int index) { return record[index] == DBNull.Value ? 0 : Convert.ToInt32(record[index]); }
        private static string ResolvePhysicalPath(string virtualPath)
        {
            if (String.IsNullOrWhiteSpace(virtualPath) || HttpContext.Current == null) return null;
            return HttpContext.Current.Server.MapPath("~/" + virtualPath.TrimStart('~', '/'));
        }
        private static string SafeFileName(string value)
        {
            value = String.IsNullOrWhiteSpace(value) ? "people-say-video" : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '-');
            return value.Length > 100 ? value.Substring(0, 100) : value;
        }
        private static string VideoMimeType(string extension)
        {
            switch ((extension ?? String.Empty).ToLowerInvariant())
            {
                case ".webm": return "video/webm";
                case ".mov": return "video/quicktime";
                default: return "video/mp4";
            }
        }
        private static void AddNullable(SqlCommand command, string name, int size, string value)
        {
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value =
                String.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
