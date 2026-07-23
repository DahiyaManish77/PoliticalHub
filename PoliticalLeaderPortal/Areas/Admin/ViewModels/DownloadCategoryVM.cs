using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class DownloadCategoryVM
    {
        public int DownloadCategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [Display(Name = "Category Name")]
        [StringLength(150)]
        public string CategoryName { get; set; }

        [Display(Name = "Category Description")]
        [StringLength(500)]
        public string CategoryDescription { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}