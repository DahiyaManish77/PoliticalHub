using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class ElectionBoothVisitVM
    {
        public int ElectionBoothVisitId { get; set; }

        public int ElectionBoothId { get; set; }

        public string BoothName { get; set; }

        [Display(Name = "Visit Type")]
        public string VisitType { get; set; }

        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; }

        public string VisitorMemberCode { get; set; }

        [Display(Name = "Visitor")]
        public string VisitorName { get; set; }

        public string TeamName { get; set; }

        public int TotalTeamMembers { get; set; }

        public int HousesVisited { get; set; }

        public int FamiliesMet { get; set; }

        public int PersonsMet { get; set; }

        public int NewSupporters { get; set; }

        public int OppositionSupporters { get; set; }

        public int NeutralFamilies { get; set; }

        public int PamphletsDistributed { get; set; }

        public int MembershipFormsIssued { get; set; }

        public int MembershipFormsCollected { get; set; }

        public int PublicComplaints { get; set; }

        public bool FollowUpRequired { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextVisitDate { get; set; }

        public string VisitStatus { get; set; }

        public string VisitStatusColor { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string VisitSummary { get; set; }

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