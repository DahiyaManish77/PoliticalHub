using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Event Vehicle Management.
    /// </summary>
    public class EventVehicleVM
    {
        public int EventVehicleId { get; set; }

        public int EventId { get; set; }

        #region Event

        public string EventCode { get; set; }

        public string EventTitle { get; set; }

        #endregion

        #region Worker

        public string WorkerCode { get; set; }

        public string WorkerName { get; set; }

        public string WorkerMobile { get; set; }

        public string Responsibility { get; set; }

        #endregion

        #region Location

        public string VillageName { get; set; }

        public string BoothName { get; set; }

        #endregion

        #region Vehicle

        public string VehicleType { get; set; }

        public int VehicleCount { get; set; }

        public int EstimatedPersons { get; set; }

        public int ActualPersons { get; set; }

        public string VehicleNumber { get; set; }

        #endregion

        #region Driver

        public string DriverName { get; set; }

        public string DriverMobile { get; set; }

        #endregion

        #region Movement

        public DateTime? ArrivalTime { get; set; }

        public DateTime? DepartureTime { get; set; }

        public string ParkingZone { get; set; }

        #endregion

        #region Expense

        public decimal FuelExpense { get; set; }

        #endregion

        #region Verification

        public bool IsVerified { get; set; }

        public int? VerifiedBy { get; set; }

        public DateTime? VerifiedDate { get; set; }

        public string VerifiedByName { get; set; }

        #endregion

        #region Remarks

        public string Remarks { get; set; }

        #endregion

        #region Audit

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedByName { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedByName { get; set; }

        #endregion
    }
}