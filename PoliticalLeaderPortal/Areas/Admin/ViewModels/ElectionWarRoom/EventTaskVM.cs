using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventTaskVM
    {
        public int EventTaskId { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        public string EventTitle { get; set; }

        public int? ParentTaskId { get; set; }

        public string ParentTaskTitle { get; set; }

        [Required(ErrorMessage = "Task title is required.")]
        [Display(Name = "Task Title")]
        public string TaskTitle { get; set; }

        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; }

        [Display(Name = "Task Category")]
        public string TaskCategory { get; set; }

        [Display(Name = "Assigned Member Code")]
        public string AssignedToMemberCode { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedToName { get; set; }

        public int? AssignedTeamId { get; set; }

        public string AssignedTeamName { get; set; }

        [Display(Name = "Assigned By")]
        public string AssignedByMemberCode { get; set; }

        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public string StatusColor { get; set; }

        public int ProgressPercentage { get; set; }

        public decimal? EstimatedHours { get; set; }

        public decimal? ActualHours { get; set; }

        public bool IsMilestone { get; set; }

        public bool RequiresApproval { get; set; }

        public string ApprovedByMemberCode { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int ActivityCount { get; set; }
    }
}