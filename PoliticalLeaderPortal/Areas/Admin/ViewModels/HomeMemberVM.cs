using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class HomeMemberVM
    {
        public int HomeMemberId { get; set; }

        [Required]
        [StringLength(160)]
        [Display(Name = "Name")]
        public string MemberName { get; set; }

        [StringLength(160)]
        public string Designation { get; set; }

        [StringLength(120)]
        public string Tenure { get; set; }

        public string PhotoPath { get; set; }

        [Display(Name = "Photo")]
        public HttpPostedFileBase PhotoFile { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Show On Home")]
        public bool ShowOnHome { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
