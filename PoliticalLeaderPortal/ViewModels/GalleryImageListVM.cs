namespace PoliticalLeaderPortal.ViewModels
{
    public class GalleryImageListVM
    {
        public int GalleryImageId { get; set; }

        public int GalleryCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string ImageTitle { get; set; }

        public string ImageCaption { get; set; }

        public string ImagePath { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}