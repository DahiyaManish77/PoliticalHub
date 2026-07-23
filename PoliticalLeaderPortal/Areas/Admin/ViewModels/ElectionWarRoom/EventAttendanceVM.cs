using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Event Attendance Management.
    /// Used for Attendance CRUD, Dashboard, Reports and Search.
    /// </summary>
    public class EventAttendanceVM
    {
        public int AttendanceId { get; set; }

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

        #region Person Information

        public string AttendanceType { get; set; }

        public string PersonName { get; set; }

        public string FatherName { get; set; }

        public string MobileNumber { get; set; }

        public string Gender { get; set; }

        public int? Age { get; set; }

        #endregion

        #region Address

        public string State { get; set; }

        public string District { get; set; }

        public string Block { get; set; }

        public string Village { get; set; }

        public string Booth { get; set; }

        #endregion

        #region Worker Information

        public string WorkerName { get; set; }

        public string WorkerCode { get; set; }

        public string MemberCode { get; set; }

        #endregion

        #region Attendance

        public string AttendanceMode { get; set; }

        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        #endregion

        #region Categories

        public bool IsVIP { get; set; }

        public bool IsVolunteer { get; set; }

        public bool IsWorker { get; set; }

        #endregion

        #region Verification

        public bool IsVerified { get; set; }

        #endregion

        #region Remarks

        public string Remarks { get; set; }

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