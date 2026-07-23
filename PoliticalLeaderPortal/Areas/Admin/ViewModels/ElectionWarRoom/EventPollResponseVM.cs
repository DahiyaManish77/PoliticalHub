using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventPollResponseVM
    {
        public int EventPollResponseId { get; set; }

        public int EventPollId { get; set; }

        public string PollTitle { get; set; }

        public int? EventPollOptionId { get; set; }

        public string OptionText { get; set; }

        public string SurveyPersonMemberCode { get; set; }

        public string SurveyPersonName { get; set; }

        [Display(Name = "Respondent")]
        public string RespondentName { get; set; }

        public string RespondentMobile { get; set; }

        public string Gender { get; set; }

        public int? Age { get; set; }

        public string State { get; set; }

        public string District { get; set; }

        public string Block { get; set; }

        public string Village { get; set; }

        public string Booth { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string ResponseText { get; set; }

        public string DeviceId { get; set; }

        public bool IsVerified { get; set; }

        public DateTime ResponseDate { get; set; }

        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}