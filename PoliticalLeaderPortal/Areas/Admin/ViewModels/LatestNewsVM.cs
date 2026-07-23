using System;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class LatestNewsVM
    {
        public int NewsId { get; set; }


    public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string ImagePath { get; set; }

        public DateTime PublishDate { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }

}
