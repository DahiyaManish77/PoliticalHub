namespace PoliticalLeaderPortal.ViewModels
{
    public class VideoGalleryListVM
    {
        public int VideoId { get; set; }

        public string CategoryName { get; set; }

        public string VideoTitle { get; set; }

        public string YoutubeUrl { get; set; }

        public string VideoFilePath { get; set; }

        public string ThumbnailImagePath { get; set; }

        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
