using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.ViewModels
{
    public class UpcomingEventVM
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Title")]
        public string Title { get; set; }

        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }

        [Display(Name = "Full Description")]
        public string FullDescription { get; set; }

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Time")]
        public string EventTime { get; set; }

        [Display(Name = "Event Location")]
        public string EventLocation { get; set; }

        public string EventImagePath { get; set; }

        public HttpPostedFileBase EventImageFile { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}