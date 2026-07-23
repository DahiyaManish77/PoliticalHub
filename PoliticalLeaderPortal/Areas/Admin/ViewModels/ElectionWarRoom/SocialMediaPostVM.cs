using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class SocialMediaPostVM
    {
        public int SocialMediaPostId { get; set; }

        [Required, StringLength(120)]
        public string Platform { get; set; }

        [Required, StringLength(180)]
        public string ContentTitle { get; set; }

        [StringLength(80)]
        public string ContentType { get; set; }

        [StringLength(600)]
        public string Caption { get; set; }

        [StringLength(300)]
        public string MediaUrl { get; set; }

        [StringLength(300)]
        public string PublicUrl { get; set; }

        public DateTime? ScheduledOn { get; set; }

        [StringLength(80)]
        public string AssignedTo { get; set; }

        [StringLength(40)]
        public string ApprovalStatus { get; set; }

        [StringLength(40)]
        public string PublishStatus { get; set; }

        public int ReachCount { get; set; }
        public int EngagementCount { get; set; }
        public int ShareCount { get; set; }
        public int CommentCount { get; set; }

        [StringLength(300)]
        public string ReviewRemarks { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
