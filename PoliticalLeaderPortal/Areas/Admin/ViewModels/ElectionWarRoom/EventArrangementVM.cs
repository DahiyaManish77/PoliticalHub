using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Event Arrangement Management.
    /// Used for CRUD, Dashboard, Reports and Search.
    /// </summary>
    public class EventArrangementVM
    {
        public int EventArrangementId { get; set; }

        public int EventId { get; set; }

        #region Event Information

        public string EventCode { get; set; }

        public string EventTitle { get; set; }

        #endregion

        #region Arrangement

        public string ArrangementType { get; set; }

        public string ArrangementName { get; set; }

        public string ResponsiblePerson { get; set; }

        public string ResponsibleMobile { get; set; }

        public string VendorName { get; set; }

        public string VendorMobile { get; set; }

        public int? Quantity { get; set; }

        #endregion

        #region Cost

        public decimal EstimatedCost { get; set; }

        public decimal ActualCost { get; set; }

        /// <summary>
        /// Difference between Actual and Estimated Cost.
        /// Positive = Over Budget
        /// Negative = Under Budget
        /// </summary>
        public decimal CostDifference
        {
            get
            {
                return ActualCost - EstimatedCost;
            }
        }

        #endregion

        #region Schedule

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        #endregion

        #region Status

        public string Status { get; set; }

        public string Priority { get; set; }

        #endregion

        #region Verification

        public bool IsVerified { get; set; }

        public int? VerifiedBy { get; set; }

        public DateTime? VerifiedDate { get; set; }

        #endregion

        #region Remarks

        public string Remarks { get; set; }

        #endregion

        #region Dashboard Fields

        /// <summary>
        /// Total duration in hours.
        /// </summary>
        public double DurationHours
        {
            get
            {
                if (!StartTime.HasValue || !EndTime.HasValue)
                {
                    return 0;
                }

                return Math.Round(
                    (EndTime.Value - StartTime.Value).TotalHours,
                    2);
            }
        }

        /// <summary>
        /// Indicates whether actual cost exceeded estimated cost.
        /// </summary>
        public bool IsOverBudget
        {
            get
            {
                return ActualCost > EstimatedCost;
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