using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Jan Sampark Management.
    /// Used for Create, Edit, Details, List and Reports.
    /// </summary>
    public class JanSamparkVM
    {
        #region Primary Key

        public int JanSamparkId { get; set; }

        #endregion

        #region Event & Booth Information

        [Display(Name = "Event")]
        public int? EventId { get; set; }

        [Display(Name = "Election Booth")]
        public int? ElectionBoothId { get; set; }

        public string EventName { get; set; }

        public string BoothName { get; set; }

        #endregion

        #region Citizen Information

        [Required]
        [StringLength(200)]
        [Display(Name = "Citizen Name")]
        public string CitizenName { get; set; }

        [StringLength(200)]
        [Display(Name = "Father Name")]
        public string FatherName { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        public int? Age { get; set; }

        #endregion

        #region Address Information

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(100)]
        public string District { get; set; }

        [StringLength(100)]
        public string Block { get; set; }

        [StringLength(150)]
        public string Village { get; set; }

        [StringLength(100)]
        public string Booth { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        #endregion

        #region Complaint Information

        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [StringLength(300)]
        public string Subject { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Priority { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        #endregion

        #region Assignment Information

        [StringLength(50)]
        public string AssignedToMemberCode { get; set; }

        [StringLength(200)]
        public string AssignedToName { get; set; }

        #endregion

        #region Resolution Information

        [DataType(DataType.MultilineText)]
        public string Resolution { get; set; }

        public DateTime? ResolutionDate { get; set; }

        public bool IsResolved { get; set; }

        #endregion

        #region Follow Up

        public bool FollowUpRequired { get; set; }

        public DateTime? FollowUpDate { get; set; }

        #endregion

        #region Attachment

        public string AttachmentPath { get; set; }

        #endregion

        #region Geo Location

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        #endregion

        #region Audit Information

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        #endregion
    }
}