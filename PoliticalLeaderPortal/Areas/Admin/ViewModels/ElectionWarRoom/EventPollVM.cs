using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventPollVM
    {
        public int EventPollId { get; set; }

        [Display(Name = "Event")]
        public int? EventId { get; set; }

        public string EventTitle { get; set; }

        [Required]
        [Display(Name = "Poll Title")]
        public string PollTitle { get; set; }

        [Display(Name = "Description")]
        public string PollDescription { get; set; }

        [Display(Name = "Poll Type")]
        public string PollType { get; set; }

        [Display(Name = "Question Type")]
        public string QuestionType { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsAnonymous { get; set; }

        public bool AllowMultipleSelection { get; set; }

        public int MaximumSelection { get; set; }

        public string Status { get; set; }

        public string StatusColor { get; set; }

        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Dashboard Helpers

        public int OptionCount { get; set; }

        public int ResponseCount { get; set; }
    }
}