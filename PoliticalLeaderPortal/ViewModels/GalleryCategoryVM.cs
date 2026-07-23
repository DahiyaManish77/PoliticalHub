using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.ViewModels
{
    public class GalleryCategoryVM
    {
        public int GalleryCategoryId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [StringLength(200)]
        public string CategoryName { get; set; }

        [Display(Name = "Description")]
        public string CategoryDescription { get; set; }

        public string CoverImagePath { get; set; }

        [Display(Name = "Cover Image")]
        public HttpPostedFileBase CoverImageFile { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}