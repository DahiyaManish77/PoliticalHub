using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Event Guest Management.
    /// Used for Guest CRUD, Dashboard, Reports, Protocol and Search.
    /// </summary>
    public class EventGuestVM
    {
        public int EventGuestId { get; set; }

        public int EventId { get; set; }

        #region Event Information

        /// <summary>
        /// Display purpose only.
        /// </summary>
        public string EventCode { get; set; }

        /// <summary>
        /// Display purpose only.
        /// </summary>
        public string EventTitle { get; set; }

        #endregion

        #region Guest Information

        public string GuestName { get; set; }

        public string Designation { get; set; }

        public string Organization { get; set; }

        public string MobileNumber { get; set; }

        public string Email { get; set; }

        public string GuestCategory { get; set; }

        #endregion

        #region Invitation

        public string InvitationStatus { get; set; }

        public string ConfirmationStatus { get; set; }

        #endregion

        #region Visit Schedule

        public DateTime? ArrivalTime { get; set; }

        public DateTime? DepartureTime { get; set; }

        #endregion

        #region Vehicle

        public bool VehicleRequired { get; set; }

        public string VehicleDetails { get; set; }

        public string DriverName { get; set; }

        public string DriverMobile { get; set; }

        #endregion

        #region Hotel

        public bool HotelRequired { get; set; }

        public string HotelName { get; set; }

        public string RoomNumber { get; set; }

        #endregion

        #region Security

        public bool SecurityRequired { get; set; }

        public string SecurityLevel { get; set; }

        #endregion

        #region Protocol

        public string StageSeatNumber { get; set; }

        public string ProtocolOfficer { get; set; }

        public string EscortOfficer { get; set; }

        #endregion

        #region Remarks

        public string Remarks { get; set; }

        #endregion

        #region Dashboard Fields

        /// <summary>
        /// Total stay duration in hours.
        /// Calculated at runtime.
        /// </summary>
        public double StayDurationHours
        {
            get
            {
                if (!ArrivalTime.HasValue || !DepartureTime.HasValue)
                {
                    return 0;
                }

                return Math.Round(
                    (DepartureTime.Value - ArrivalTime.Value).TotalHours,
                    2);
            }
        }

        /// <summary>
        /// Indicates whether the guest has confirmed attendance.
        /// </summary>
        public bool IsConfirmed
        {
            get
            {
                return !string.IsNullOrEmpty(ConfirmationStatus) &&
                       ConfirmationStatus.Equals(
                           "Confirmed",
                           StringComparison.OrdinalIgnoreCase);
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