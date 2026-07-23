using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.ViewModels
{
    public class CitizenConnectVM
    {
        public int CitizenConnectId { get; set; }

        [Required]
        [StringLength(40)]
        public string RequestType { get; set; }

        [Required]
        [StringLength(160)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [StringLength(160)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(160)]
        public string District { get; set; }

        [StringLength(160)]
        public string Assembly { get; set; }

        [Required]
        [StringLength(250)]
        public string Subject { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [StringLength(40)]
        public string Status { get; set; }

        public string AdminRemarks { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
