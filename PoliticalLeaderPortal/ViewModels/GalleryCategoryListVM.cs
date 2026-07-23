namespace PoliticalLeaderPortal.ViewModels
{
    public class GalleryCategoryListVM
    {
        public int GalleryCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public string CoverImagePath { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public int TotalImages { get; set; }
    }
}