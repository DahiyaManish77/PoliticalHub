using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Services
{
    public class VideoGalleryService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public VideoGalleryService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        #region Video Category

        public List<VideoCategoryListVM> GetAllCategories()
        {
            return _db.VideoCategories
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new VideoCategoryListVM
                {
                    VideoCategoryId = x.VideoCategoryId,
                    CategoryName = x.CategoryName,
                    CategoryDescription = x.CategoryDescription,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive,
                    TotalVideos = x.VideoGalleries.Count()
                })
                .ToList();
        }

        public VideoCategoryVM GetCategoryById(int id)
        {
            var entity = _db.VideoCategories
                .FirstOrDefault(x => x.VideoCategoryId == id);

            if (entity == null)
                return null;

            return new VideoCategoryVM
            {
                VideoCategoryId = entity.VideoCategoryId,
                CategoryName = entity.CategoryName,
                CategoryDescription = entity.CategoryDescription,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public void CreateCategory(VideoCategoryVM model)
        {
            var entity = new VideoCategory();

            entity.CategoryName = model.CategoryName;
            entity.CategoryDescription = model.CategoryDescription;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.CreatedDate = DateTime.Now;

            _db.VideoCategories.Add(entity);
            _db.SaveChanges();
        }

        public void UpdateCategory(VideoCategoryVM model)
        {
            var entity = _db.VideoCategories
                .FirstOrDefault(x => x.VideoCategoryId == model.VideoCategoryId);

            if (entity == null)
                return;

            entity.CategoryName = model.CategoryName;
            entity.CategoryDescription = model.CategoryDescription;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();
        }

        public void DeleteCategory(int id)
        {
            var entity = _db.VideoCategories
                .FirstOrDefault(x => x.VideoCategoryId == id);

            if (entity == null)
                return;

            _db.VideoCategories.Remove(entity);
            _db.SaveChanges();
        }

        #endregion

        #region Video Gallery

        public List<VideoGalleryListVM> GetAllVideos()
        {
            EnsureVideoFileColumn();

            return _db.VideoGalleries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new VideoGalleryListVM
                {
                    VideoId = x.VideoId,
                    CategoryName = x.VideoCategory.CategoryName,
                    VideoTitle = x.VideoTitle,
                    YoutubeUrl = x.YoutubeUrl,
                    VideoFilePath = "",
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    IsFeatured = x.IsFeatured,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive
                })
                .ToList()
                .Select(AttachVideoFilePath)
                .ToList();
        }

        public VideoGalleryVM GetVideoById(int id)
        {
            EnsureVideoFileColumn();

            var entity = _db.VideoGalleries
                .FirstOrDefault(x => x.VideoId == id);

            if (entity == null)
                return null;

            return new VideoGalleryVM
            {
                VideoId = entity.VideoId,
                VideoCategoryId = entity.VideoCategoryId,
                VideoTitle = entity.VideoTitle,
                VideoDescription = entity.VideoDescription,
                YoutubeUrl = entity.YoutubeUrl,
                VideoFilePath = GetVideoFilePath(entity.VideoId),
                ThumbnailImagePath = entity.ThumbnailImagePath,
                DisplayOrder = entity.DisplayOrder,
                IsFeatured = entity.IsFeatured,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                Categories = GetCategoryDropdown()
            };
        }
        public List<HomeVideoGalleryVM> GetHomepageVideos(int count)
        {
            EnsureVideoFileColumn();
            TryAutoSyncYouTubeVideos();

            return _db.VideoGalleries
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.IsFeatured)
                .ThenBy(x => x.DisplayOrder)
                .Take(count)
                .Select(x => new HomeVideoGalleryVM
                {
                    VideoId = x.VideoId,
                    VideoTitle = x.VideoTitle,
                    VideoDescription = x.VideoDescription,
                    YoutubeUrl = x.YoutubeUrl,
                    VideoFilePath = "",
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    CategoryName = x.VideoCategory.CategoryName,
                    IsFeatured = x.IsFeatured
                })
                .ToList()
                .Select(AttachVideoFilePath)
                .ToList();
        }
        public VideoGalleryVM GetPublicVideo(int id)
        {
            EnsureVideoFileColumn();

            var entity = _db.VideoGalleries
                .FirstOrDefault(x =>
                    x.VideoId == id &&
                    x.IsActive);

            if (entity == null)
                return null;

            return new VideoGalleryVM
            {
                VideoId = entity.VideoId,
                VideoCategoryId = entity.VideoCategoryId,
                VideoTitle = entity.VideoTitle,
                VideoDescription = entity.VideoDescription,
                YoutubeUrl = entity.YoutubeUrl,
                VideoFilePath = GetVideoFilePath(entity.VideoId),
                ThumbnailImagePath = entity.ThumbnailImagePath,
                IsFeatured = entity.IsFeatured,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
            };
        }
        public void CreateVideo(VideoGalleryVM model)
        {
            EnsureVideoFileColumn();

            var entity = new VideoGallery();

            entity.VideoCategoryId = model.VideoCategoryId;
            entity.VideoTitle = model.VideoTitle;
            entity.VideoDescription = model.VideoDescription;
            entity.YoutubeUrl = model.YoutubeUrl;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsFeatured = model.IsFeatured;
            entity.IsActive = model.IsActive;
            entity.CreatedDate = DateTime.Now;

            if (model.ThumbnailImageFile != null)
            {
                entity.ThumbnailImagePath =
                    SaveThumbnail(model.ThumbnailImageFile);
            }

            _db.VideoGalleries.Add(entity);
            _db.SaveChanges();

            SaveVideoFilePath(entity.VideoId, model);
        }

        public void UpdateVideo(VideoGalleryVM model)
        {
            EnsureVideoFileColumn();

            var entity = _db.VideoGalleries
                .FirstOrDefault(x => x.VideoId == model.VideoId);

            if (entity == null)
                return;

            entity.VideoCategoryId = model.VideoCategoryId;
            entity.VideoTitle = model.VideoTitle;
            entity.VideoDescription = model.VideoDescription;
            entity.YoutubeUrl = model.YoutubeUrl;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsFeatured = model.IsFeatured;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.Now;

            if (model.ThumbnailImageFile != null)
            {
                entity.ThumbnailImagePath =
                    SaveThumbnail(model.ThumbnailImageFile);
            }

            _db.SaveChanges();

            SaveVideoFilePath(entity.VideoId, model);
        }

        public void DeleteVideo(int id)
        {
            var entity = _db.VideoGalleries
                .FirstOrDefault(x => x.VideoId == id);

            if (entity == null)
                return;

            _db.VideoGalleries.Remove(entity);
            _db.SaveChanges();
        }

        public int SyncYouTubeVideos()
        {
            EnsureVideoFileColumn();
            ClearYouTubeSyncCache();

            string apiKey = ConfigurationManager.AppSettings["YouTubeApiKey"];
            string channelId = ConfigurationManager.AppSettings["YouTubeChannelId"];

            if (String.IsNullOrWhiteSpace(apiKey) ||
                String.IsNullOrWhiteSpace(channelId))
            {
                throw new InvalidOperationException("YouTube API key or channel id is missing in Web.config appSettings.");
            }

            int categoryId = EnsureYouTubeCategory();
            int imported = 0;
            int scanned = 0;
            int maxVideos = GetConfiguredInt("YouTubeAutoSyncMaxVideos", 200);

            using (var client = new WebClient())
            {
                string playlistId = GetConfiguredPlaylistId(client, apiKey, channelId);

                if (!String.IsNullOrWhiteSpace(playlistId))
                {
                    imported = ImportYouTubePlaylistVideos(client, apiKey, playlistId, categoryId, maxVideos, out scanned);
                }

                if (scanned == 0)
                {
                    imported = ImportYouTubeChannelVideos(client, apiKey, channelId, categoryId, maxVideos, out scanned);
                }
            }

            _db.SaveChanges();

            return imported;
        }

        private int ImportYouTubePlaylistVideos(WebClient client, string apiKey, string playlistId, int categoryId, int maxVideos, out int scanned)
        {
            int imported = 0;
            scanned = 0;
            string nextPageToken = null;

            do
            {
                string url =
                    "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=" +
                    Uri.EscapeDataString(playlistId) +
                    "&maxResults=50&key=" +
                    Uri.EscapeDataString(apiKey);

                if (!String.IsNullOrWhiteSpace(nextPageToken))
                {
                    url += "&pageToken=" + Uri.EscapeDataString(nextPageToken);
                }

                string json = client.DownloadString(url);
                var root = JObject.Parse(json);

                foreach (var item in root["items"] ?? new JArray())
                {
                    if (scanned >= maxVideos) break;
                    scanned++;

                    string videoId = Convert.ToString(item["snippet"]?["resourceId"]?["videoId"]);
                    var snippet = item["snippet"];
                    imported += AddYouTubeVideoIfMissing(videoId, snippet, categoryId, scanned);
                }

                nextPageToken = Convert.ToString(root["nextPageToken"]);
            }
            while (!String.IsNullOrWhiteSpace(nextPageToken) && scanned < maxVideos);

            return imported;
        }

        private int ImportYouTubeChannelVideos(WebClient client, string apiKey, string channelId, int categoryId, int maxVideos, out int scanned)
        {
            int imported = 0;
            scanned = 0;
            string nextPageToken = null;

            do
            {
                string url =
                    "https://www.googleapis.com/youtube/v3/search?part=snippet&channelId=" +
                    Uri.EscapeDataString(channelId) +
                    "&maxResults=50&order=date&type=video&key=" +
                    Uri.EscapeDataString(apiKey);

                if (!String.IsNullOrWhiteSpace(nextPageToken))
                {
                    url += "&pageToken=" + Uri.EscapeDataString(nextPageToken);
                }

                string json = client.DownloadString(url);
                var root = JObject.Parse(json);

                foreach (var item in root["items"] ?? new JArray())
                {
                    if (scanned >= maxVideos) break;
                    scanned++;

                    string videoId = Convert.ToString(item["id"]?["videoId"]);
                    var snippet = item["snippet"];
                    imported += AddYouTubeVideoIfMissing(videoId, snippet, categoryId, scanned);
                }

                nextPageToken = Convert.ToString(root["nextPageToken"]);
            }
            while (!String.IsNullOrWhiteSpace(nextPageToken) && scanned < maxVideos);

            return imported;
        }

        private int AddYouTubeVideoIfMissing(string videoId, JToken snippet, int categoryId, int displayOrder)
        {
            if (String.IsNullOrWhiteSpace(videoId))
            {
                return 0;
            }

            string youtubeUrl = "https://www.youtube.com/watch?v=" + videoId;

            if (_db.VideoGalleries.Any(x => x.YoutubeUrl == youtubeUrl))
            {
                return 0;
            }

            string thumbnail =
                Convert.ToString(snippet?["thumbnails"]?["high"]?["url"]) ??
                Convert.ToString(snippet?["thumbnails"]?["medium"]?["url"]) ??
                Convert.ToString(snippet?["thumbnails"]?["default"]?["url"]);

            _db.VideoGalleries.Add(new VideoGallery
            {
                VideoCategoryId = categoryId,
                VideoTitle = Convert.ToString(snippet?["title"]),
                VideoDescription = Convert.ToString(snippet?["description"]),
                YoutubeUrl = youtubeUrl,
                ThumbnailImagePath = thumbnail,
                DisplayOrder = displayOrder,
                IsFeatured = displayOrder == 1 && !_db.VideoGalleries.Any(x => x.IsFeatured),
                IsActive = true,
                CreatedDate = DateTime.Now
            });

            return 1;
        }

        private string GetConfiguredPlaylistId(WebClient client, string apiKey, string channelId)
        {
            string playlistId = ConfigurationManager.AppSettings["YouTubePlaylistId"];

            if (!String.IsNullOrWhiteSpace(playlistId))
            {
                return playlistId.Trim();
            }

            string playlistName = ConfigurationManager.AppSettings["YouTubePlaylistName"];

            if (!String.IsNullOrWhiteSpace(playlistName))
            {
                string namedPlaylistId = FindPlaylistIdByName(client, apiKey, channelId, playlistName);

                if (!String.IsNullOrWhiteSpace(namedPlaylistId))
                {
                    return namedPlaylistId;
                }
            }

            return GetChannelUploadsPlaylistId(client, apiKey, channelId);
        }

        private string FindPlaylistIdByName(WebClient client, string apiKey, string channelId, string playlistName)
        {
            string targetName = NormalizePlaylistName(playlistName);

            string nextPageToken = null;

            do
            {
                string url =
                    "https://www.googleapis.com/youtube/v3/playlists?part=snippet&channelId=" +
                    Uri.EscapeDataString(channelId) +
                    "&maxResults=50&key=" +
                    Uri.EscapeDataString(apiKey);

                if (!String.IsNullOrWhiteSpace(nextPageToken))
                {
                    url += "&pageToken=" + Uri.EscapeDataString(nextPageToken);
                }

                string json = client.DownloadString(url);
                var root = JObject.Parse(json);

                foreach (var item in root["items"] ?? new JArray())
                {
                    string title = Convert.ToString(item["snippet"]?["title"]);

                    if (String.Equals(NormalizePlaylistName(title), targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Convert.ToString(item["id"]);
                    }
                }

                nextPageToken = Convert.ToString(root["nextPageToken"]);
            }
            while (!String.IsNullOrWhiteSpace(nextPageToken));

            return null;
        }

        private string GetChannelUploadsPlaylistId(WebClient client, string apiKey, string channelId)
        {
            string url =
                "https://www.googleapis.com/youtube/v3/channels?part=contentDetails&id=" +
                Uri.EscapeDataString(channelId) +
                "&key=" +
                Uri.EscapeDataString(apiKey);

            string json = client.DownloadString(url);
            var root = JObject.Parse(json);
            var first = (root["items"] ?? new JArray()).FirstOrDefault();

            return Convert.ToString(first?["contentDetails"]?["relatedPlaylists"]?["uploads"]);
        }

        private string NormalizePlaylistName(string value)
        {
            return String.IsNullOrWhiteSpace(value)
                ? String.Empty
                : value.Trim().Replace(" ", String.Empty);
        }

        #endregion

        #region Dropdown

        public List<SelectListItem> GetCategoryDropdown()
        {
            return _db.VideoCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new SelectListItem
                {
                    Value = x.VideoCategoryId.ToString(),
                    Text = x.CategoryName
                })
                .ToList();
        }

        #endregion

        #region Public

       

        public List<VideoGalleryListVM> GetPublicVideos()
        {
            EnsureVideoFileColumn();
            TryAutoSyncYouTubeVideos();

            return _db.VideoGalleries
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.IsFeatured)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new VideoGalleryListVM
                {
                    VideoId = x.VideoId,
                    CategoryName = x.VideoCategory.CategoryName,
                    VideoTitle = x.VideoTitle,
                    YoutubeUrl = x.YoutubeUrl,
                    VideoFilePath = "",
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    IsFeatured = x.IsFeatured
                })
                .ToList()
                .Select(AttachVideoFilePath)
                .ToList();
        }

        #endregion

        #region Helper

        private void TryAutoSyncYouTubeVideos()
        {
            string enabled = ConfigurationManager.AppSettings["YouTubeAutoSyncEnabled"];

            if (!String.IsNullOrWhiteSpace(enabled) &&
                String.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string cacheKey = "PoliticalLeaderPortal.YouTube.LastSync";

            if (HttpRuntime.Cache[cacheKey] != null)
            {
                return;
            }

            try
            {
                SyncYouTubeVideos();

                int minutes = 60;
                int configuredMinutes;

                if (Int32.TryParse(ConfigurationManager.AppSettings["YouTubeAutoSyncMinutes"], out configuredMinutes) &&
                    configuredMinutes > 0)
                {
                    minutes = configuredMinutes;
                }

                HttpRuntime.Cache.Insert(
                    cacheKey,
                    DateTime.Now,
                    null,
                    DateTime.Now.AddMinutes(minutes),
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }
            catch
            {
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    DateTime.Now,
                    null,
                    DateTime.Now.AddMinutes(15),
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }
        }

        private void ClearYouTubeSyncCache()
        {
            HttpRuntime.Cache.Remove("PoliticalLeaderPortal.YouTube.LastSync");
        }

        private int GetConfiguredInt(string key, int fallback)
        {
            int value;
            return Int32.TryParse(ConfigurationManager.AppSettings[key], out value) && value > 0
                ? value
                : fallback;
        }

        private string SaveThumbnail(HttpPostedFileBase file)
        {
            if (file == null)
                return null;

            string fileName =
                Guid.NewGuid().ToString() +
                Path.GetExtension(file.FileName);

            string folder =
                HttpContext.Current.Server.MapPath("~/Uploads/Videos/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fullPath =
                Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Uploads/Videos/" + fileName;
        }

        private int EnsureYouTubeCategory()
        {
            var category =
                _db.VideoCategories
                .FirstOrDefault(x => x.CategoryName == "YouTube");

            if (category != null)
            {
                return category.VideoCategoryId;
            }

            category = new VideoCategory
            {
                CategoryName = "YouTube",
                CategoryDescription = "Automatically imported YouTube videos",
                DisplayOrder = 0,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _db.VideoCategories.Add(category);
            _db.SaveChanges();

            return category.VideoCategoryId;
        }

        private void EnsureVideoFileColumn()
        {
            _db.Database.ExecuteSqlCommand(@"
IF COL_LENGTH('dbo.VideoGallery', 'VideoFilePath') IS NULL
    ALTER TABLE dbo.VideoGallery ADD VideoFilePath NVARCHAR(500) NULL;");
        }

        private VideoGalleryListVM AttachVideoFilePath(VideoGalleryListVM model)
        {
            model.VideoFilePath = GetVideoFilePath(model.VideoId);
            return model;
        }

        private HomeVideoGalleryVM AttachVideoFilePath(HomeVideoGalleryVM model)
        {
            model.VideoFilePath = GetVideoFilePath(model.VideoId);
            return model;
        }

        private string GetVideoFilePath(int videoId)
        {
            try
            {
                return _db.Database.SqlQuery<string>(
                    "SELECT ISNULL(VideoFilePath, '') FROM dbo.VideoGallery WHERE VideoId = @p0",
                    videoId)
                    .FirstOrDefault();
            }
            catch
            {
                return String.Empty;
            }
        }

        private void SaveVideoFilePath(int videoId, VideoGalleryVM model)
        {
            string videoFilePath = model.VideoFilePath;

            if (model.VideoFile != null &&
                model.VideoFile.ContentLength > 0)
            {
                videoFilePath = SaveVideoFile(model.VideoFile);
            }

            _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.VideoGallery SET VideoFilePath = @p0 WHERE VideoId = @p1",
                (object)(videoFilePath ?? String.Empty),
                videoId);
        }

        private string SaveVideoFile(HttpPostedFileBase file)
        {
            if (file == null ||
                file.ContentLength <= 0)
            {
                return null;
            }

            string extension = Path.GetExtension(file.FileName);
            string[] allowedExtensions = { ".mp4", ".webm", ".mov" };

            if (String.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                throw new InvalidOperationException("Only MP4, WEBM and MOV videos are allowed.");
            }

            if (file.ContentLength > 100 * 1024 * 1024)
            {
                throw new InvalidOperationException("Video file size must be less than 100 MB.");
            }

            string fileName =
                "video_" +
                Guid.NewGuid().ToString("N") +
                extension;

            string folder =
                HttpContext.Current.Server.MapPath("~/Uploads/Videos/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fullPath =
                Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Uploads/Videos/" + fileName;
        }

        #endregion
    }
}
