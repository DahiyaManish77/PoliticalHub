using System;

namespace PoliticalLeaderPortal.ViewModels
{
    public class UpcomingEventDetailsVM
    {
        public int EventId { get; set; }

        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public DateTime EventDate { get; set; }

        public string EventTime { get; set; }

        public string EventLocation { get; set; }

        public string EventImagePath { get; set; }
    }
}