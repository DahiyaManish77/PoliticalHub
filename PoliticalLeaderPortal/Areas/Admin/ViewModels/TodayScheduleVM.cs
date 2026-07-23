using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class TodayScheduleVM
    {
        public int TodayScheduleId { get; set; }

        [Required]
        [Display(Name = "Schedule Date")]
        public DateTime ScheduleDate { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Time")]
        public string ScheduleTime { get; set; }

        [Required]
        [StringLength(250)]
        public string Place { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(150)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact Mobile")]
        public string ContactMobile { get; set; }

        [StringLength(100)]
        [Display(Name = "Organizer")]
        public string OrganizerName { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(50)]
        public string Priority { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(500)]
        [Display(Name = "Map Link")]
        public string MapLink { get; set; }

        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
