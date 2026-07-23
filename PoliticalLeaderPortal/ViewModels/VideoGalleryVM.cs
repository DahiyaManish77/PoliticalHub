using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.ViewModels
{
    public class VideoGalleryVM
    {
        public int VideoId { get; set; }

        [Required]
        [Display(Name = "Video Category")]
        public int VideoCategoryId { get; set; }

        [Required]
        [Display(Name = "Video Title")]
        public string VideoTitle { get; set; }

        [Display(Name = "Video Description")]
        public string VideoDescription { get; set; }

        [Display(Name = "Video / Page URL")]
        public string YoutubeUrl { get; set; }

        public string VideoFilePath { get; set; }

        public string ThumbnailImagePath { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Featured Video")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        public HttpPostedFileBase ThumbnailImageFile { get; set; }

        public HttpPostedFileBase VideoFile { get; set; }
    }
}
