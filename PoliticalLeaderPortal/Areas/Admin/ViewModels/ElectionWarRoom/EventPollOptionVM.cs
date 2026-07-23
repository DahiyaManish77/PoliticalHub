using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventPollOptionVM
    {
        public int EventPollOptionId { get; set; }

        public int EventPollId { get; set; }

        public string PollTitle { get; set; }

        [Required]
        [Display(Name = "Option")]
        public string OptionText { get; set; }

        public int DisplayOrder { get; set; }

        public int VoteCount { get; set; }

        public decimal VotePercentage { get; set; }

        public bool IsWinner { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}