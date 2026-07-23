using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.ViewModels
{
    public class MeraKshetraItemVM
    {
        public int MeraKshetraItemId { get; set; }

        [Required]
        [StringLength(80)]
        public string ModuleType { get; set; }

        [Required]
        [StringLength(160)]
        public string Title { get; set; }

        [StringLength(220)]
        public string ShortTitle { get; set; }

        public string Description { get; set; }

        [StringLength(500)]
        public string ImagePath { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        [StringLength(80)]
        public string IconClass { get; set; }

        [StringLength(120)]
        public string LocationName { get; set; }

        [StringLength(80)]
        public string DistanceText { get; set; }

        [StringLength(500)]
        public string SourceUrl { get; set; }

        [StringLength(80)]
        public string SourceName { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
