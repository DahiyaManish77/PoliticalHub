using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class PublicNewsTickerVM
    {
        public int PublicNewsTickerId { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Ticker Text")]
        public string TickerText { get; set; }

        [StringLength(500)]
        [Display(Name = "Link Url")]
        public string LinkUrl { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
