using System;
using System.Collections.Generic;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventVM
    {
        public EventVM()
        {
            Vehicles = new List<EventVehicleVM>();
            Attendance = new List<EventAttendanceVM>();
            Teams = new List<EventTeamVM>();
            Guests = new List<EventGuestVM>();
            Arrangements = new List<EventArrangementVM>();
            Expenses = new List<EventExpenseVM>();
            MediaFiles = new List<EventMediaVM>();
            Tasks = new List<EventTaskVM>();
            Polls = new List<EventPollVM>();
        }

        public int EventId { get; set; }
        public string EventCode { get; set; }
        public string EventTitle { get; set; }
        public string SubTitle { get; set; }
        public string EventType { get; set; }
        public string EventScope { get; set; }
        public bool ShowOnHome { get; set; }
        public bool ShowInElectionWarRoom { get; set; }
        public bool IsConfidential { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Block { get; set; }
        public string Village { get; set; }
        public string Booth { get; set; }
        public string Venue { get; set; }
        public string Landmark { get; set; }
        public string FullAddress { get; set; }
        public string GoogleMapLink { get; set; }
        public string EventImagePath { get; set; }
        public HttpPostedFileBase EventImageFile { get; set; }
        public int ExpectedCrowd { get; set; }
        public int ActualCrowd { get; set; }
        public int ExpectedVehicles { get; set; }
        public int ActualVehicles { get; set; }
        public int ExpectedVolunteers { get; set; }
        public int ActualVolunteers { get; set; }
        public int ExpectedFoodPlates { get; set; }
        public int ActualFoodPlates { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualExpense { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerMobile { get; set; }
        public string ResponsiblePerson { get; set; }
        public string ResponsibleMobile { get; set; }
        public string TransportResponsible { get; set; }
        public string FoodResponsible { get; set; }
        public string MediaResponsible { get; set; }
        public string VolunteerResponsible { get; set; }
        public string ChiefGuest { get; set; }
        public string ChiefGuestMobile { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }

        public List<EventVehicleVM> Vehicles { get; set; }
        public List<EventAttendanceVM> Attendance { get; set; }
        public List<EventTeamVM> Teams { get; set; }
        public List<EventGuestVM> Guests { get; set; }
        public List<EventArrangementVM> Arrangements { get; set; }
        public List<EventExpenseVM> Expenses { get; set; }
        public List<EventMediaVM> MediaFiles { get; set; }
        public List<EventTaskVM> Tasks { get; set; }
        public List<EventPollVM> Polls { get; set; }
    }
}
