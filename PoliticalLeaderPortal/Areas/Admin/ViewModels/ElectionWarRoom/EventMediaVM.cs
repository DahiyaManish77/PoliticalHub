using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventMediaVM
    {
        public int EventMediaId { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        public string EventTitle { get; set; }

        [Required(ErrorMessage = "Media type is required.")]
        [Display(Name = "Media Type")]
        public string MediaType { get; set; }

        [Display(Name = "Media Category")]
        public string MediaCategory { get; set; }

        [Display(Name = "Stored File Name")]
        public string FileName { get; set; }

        [Display(Name = "Original File Name")]
        public string OriginalFileName { get; set; }

        [Display(Name = "File Path")]
        public string FilePath { get; set; }

        [Display(Name = "File Extension")]
        public string FileExtension { get; set; }

        [Display(Name = "File Size (Bytes)")]
        public long? FileSize { get; set; }

        public string FileSizeText { get; set; }

        [Display(Name = "Thumbnail")]
        public string ThumbnailPath { get; set; }

        [Display(Name = "Caption")]
        public string Caption { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Uploaded By")]
        public string UploadedBy { get; set; }

        [Display(Name = "Uploaded Date")]
        [DataType(DataType.DateTime)]
        public DateTime UploadedDate { get; set; }

        [Display(Name = "Primary Media")]
        public bool IsPrimary { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Media Status")]
        public string MediaStatus { get; set; }

        // UI helper (not stored in database)
        public string MediaStatusColor { get; set; }

        [Display(Name = "Verified By")]
        public int? VerifiedBy { get; set; }

        public string VerifiedByName { get; set; }

        [Display(Name = "Verified Date")]
        [DataType(DataType.DateTime)]
        public DateTime? VerifiedDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Updated By")]
        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        [Display(Name = "Updated Date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        // UI helper properties (not stored in database)
        public string MediaIcon { get; set; }

        public bool IsImage { get; set; }

        public bool IsVideo { get; set; }

        public bool IsDocument { get; set; }

        public bool IsAudio { get; set; }
    }
}