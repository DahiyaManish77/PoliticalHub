using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class VoterRollPdfVM
    {
        public int VoterRollPdfId { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(100)]
        public string District { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Assembly Constituency")]
        public string AssemblyConstituency { get; set; }

        [StringLength(150)]
        [Display(Name = "Parliament Constituency")]
        public string ParliamentConstituency { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Part / Booth Number")]
        public string PartNumber { get; set; }

        [StringLength(250)]
        [Display(Name = "Booth Name")]
        public string BoothName { get; set; }

        [StringLength(150)]
        public string Village { get; set; }

        [StringLength(150)]
        public string Ward { get; set; }

        [StringLength(300)]
        [Display(Name = "Polling Station")]
        public string PollingStation { get; set; }

        [StringLength(500)]
        [Display(Name = "Official Source URL")]
        public string SourceUrl { get; set; }

        [StringLength(500)]
        public string PdfFilePath { get; set; }

        [Display(Name = "Roll Year")]
        public int RollYear { get; set; }

        [Display(Name = "Revision Type")]
        [StringLength(100)]
        public string RevisionType { get; set; }

        [Display(Name = "Published Date")]
        public DateTime? PublishedDate { get; set; }

        [Display(Name = "Download Date")]
        public DateTime DownloadDate { get; set; }

        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public HttpPostedFileBase PdfFile { get; set; }
    }
}
