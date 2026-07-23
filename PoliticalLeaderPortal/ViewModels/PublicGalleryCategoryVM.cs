namespace PoliticalLeaderPortal.ViewModels
{
    public class PublicGalleryCategoryVM
    {
        public int GalleryCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public string CoverImagePath { get; set; }

        public int TotalImages { get; set; }
    }
}