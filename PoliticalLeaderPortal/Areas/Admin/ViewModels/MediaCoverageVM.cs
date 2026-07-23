using System;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class MediaCoverageVM
    {
        public int MediaCoverageId { get; set; }

        public string SourceName { get; set; }

        public string SourceLogoPath { get; set; }

        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string CoverImagePath { get; set; }

        public DateTime? CoverageDate { get; set; }

        public string ExternalUrl { get; set; }

        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }

        public int ViewCount { get; set; }

        public bool IsActive { get; set; }

        public HttpPostedFileBase SourceLogoFile { get; set; }

        public HttpPostedFileBase CoverImageFile { get; set; }
    }
}