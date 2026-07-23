namespace PoliticalLeaderPortal.ViewModels
{
    public class VideoCategoryListVM
    {
        public int VideoCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public int TotalVideos { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}