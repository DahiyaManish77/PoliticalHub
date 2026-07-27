using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.Poll
{
    public class PollCategoryVM
    {
        public int PollCategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(150)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string CategoryDescription { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
