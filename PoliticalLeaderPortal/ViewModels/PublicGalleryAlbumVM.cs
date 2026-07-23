using System.Collections.Generic;

namespace PoliticalLeaderPortal.ViewModels
{
    public class PublicGalleryAlbumVM
    {
        public int GalleryCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public string CoverImagePath { get; set; }

        public List<PublicGalleryImageVM> Images { get; set; }

        public PublicGalleryAlbumVM()
        {
            Images = new List<PublicGalleryImageVM>();
        }
    }
}