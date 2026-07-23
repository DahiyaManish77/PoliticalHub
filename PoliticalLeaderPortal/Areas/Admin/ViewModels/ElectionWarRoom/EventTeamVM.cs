using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Event Team Management.
    /// Used for Team CRUD, Dashboard, Reports and Search.
    /// </summary>
    public class EventTeamVM
    {
        public int EventTeamId { get; set; }

        public int EventId { get; set; }

        #region Event Information

        /// <summary>
        /// Display Purpose Only.
        /// </summary>
        public string EventCode { get; set; }

        /// <summary>
        /// Display Purpose Only.
        /// </summary>
        public string EventTitle { get; set; }

        #endregion

        #region Team Information

        public string TeamName { get; set; }

        public string TeamLeaderName { get; set; }

        public string TeamLeaderMobile { get; set; }

        public int TotalMembers { get; set; }

        public int RequiredMembers { get; set; }

        public string AssignedArea { get; set; }

        #endregion

        #region Duty Schedule

        public DateTime? DutyStartTime { get; set; }

        public DateTime? DutyEndTime { get; set; }

        #endregion

        #region Status

        public string Status { get; set; }

        public string Priority { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedDate { get; set; }

        #endregion

        #region Instructions

        public string Instructions { get; set; }

        public string Remarks { get; set; }

        #endregion

        #region Dashboard Calculated Fields

        /// <summary>
        /// Current number of active team members.
        /// Populated from EventTeamMembers table.
        /// </summary>
        public int ActiveMembers { get; set; }

        /// <summary>
        /// Remaining members required.
        /// </summary>
        public int PendingMembers
        {
            get
            {
                return RequiredMembers - TotalMembers;
            }
        }

        /// <summary>
        /// Team completion percentage.
        /// </summary>
        public decimal CompletionPercentage
        {
            get
            {
                if (RequiredMembers == 0)
                {
                    return 0;
                }

                return Math.Round(
                    ((decimal)TotalMembers / RequiredMembers) * 100,
                    2);
            }
        }

        #endregion

        #region Audit

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        #endregion
    }
}