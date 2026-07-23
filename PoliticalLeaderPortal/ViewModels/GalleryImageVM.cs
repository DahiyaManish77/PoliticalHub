using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.ViewModels
{
    public class GalleryImageVM
    {
        public int GalleryImageId { get; set; }

        [Required]
        [Display(Name = "Gallery Category")]
        public int GalleryCategoryId { get; set; }

        [Display(Name = "Image Title")]
        [StringLength(250)]
        public string ImageTitle { get; set; }

        [Display(Name = "Image Caption")]
        public string ImageCaption { get; set; }

        public string ImagePath { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        /* Create Screen */
        public HttpPostedFileBase[] ImageFiles { get; set; }

        /* Edit Screen */
        public HttpPostedFileBase ImageFile { get; set; }
    }
}