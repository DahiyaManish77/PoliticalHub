using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class ElectionBoothVM
    {
        public int ElectionBoothId { get; set; }

        [Required]
        [Display(Name = "Booth Code")]
        public string BoothCode { get; set; }

        [Required]
        [Display(Name = "Booth Number")]
        public string BoothNumber { get; set; }

        [Required]
        [Display(Name = "Booth Name")]
        public string BoothName { get; set; }

        [Display(Name = "Assembly")]
        public string AssemblyName { get; set; }

        [Display(Name = "Parliament")]
        public string ParliamentName { get; set; }

        public string State { get; set; }

        public string District { get; set; }

        public string Block { get; set; }

        public string Village { get; set; }

        [Display(Name = "Polling Station")]
        public string PollingStation { get; set; }

        [Display(Name = "Booth Incharge Code")]
        public string BoothInchargeMemberCode { get; set; }

        [Display(Name = "Booth Incharge")]
        public string BoothInchargeName { get; set; }

        public int TotalVoters { get; set; }

        public int MaleVoters { get; set; }

        public int FemaleVoters { get; set; }

        public int OtherVoters { get; set; }

        public int Supporters { get; set; }

        public int Opponents { get; set; }

        public int NeutralVoters { get; set; }

        public int VisitedHouses { get; set; }

        public int TotalHouses { get; set; }

        public decimal CoveragePercentage { get; set; }

        public string BoothStrength { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastVisitDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastMeetingDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastSurveyDate { get; set; }

        public string Priority { get; set; }

        public string PriorityColor { get; set; }

        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Dashboard Helpers

        public int VisitCount { get; set; }

        public decimal SupportPercentage { get; set; }

        public decimal OppositionPercentage { get; set; }

        public decimal NeutralPercentage { get; set; }
    }
}