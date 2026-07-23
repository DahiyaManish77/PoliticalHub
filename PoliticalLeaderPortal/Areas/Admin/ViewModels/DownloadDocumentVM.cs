using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class DownloadDocumentVM
    {
        public int DownloadDocumentId { get; set; }

        [Required(ErrorMessage = "Please select category.")]
        [Display(Name = "Category")]
        public int DownloadCategoryId { get; set; }

        public string CategoryName { get; set; }

        [Display(Name = "Document Number")]
        [StringLength(100)]
        public string DocumentNumber { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [Display(Name = "Document Title")]
        [StringLength(250)]
        public string Title { get; set; }

        [AllowHtml]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }

        [Display(Name = "Upload Document")]
        public HttpPostedFileBase File { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string FileExtension { get; set; }

        public decimal FileSizeKB { get; set; }

        [Display(Name = "Version No.")]
        [StringLength(20)]
        public string VersionNo { get; set; }

        public int DownloadsCount { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Publish Date")]
        [DataType(DataType.Date)]
        public DateTime? PublishDate { get; set; }

        [Display(Name = "Featured Document")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}