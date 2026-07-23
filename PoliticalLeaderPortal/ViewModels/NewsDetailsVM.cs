using System;
using System.Collections.Generic;

namespace PoliticalLeaderPortal.ViewModels
{
    public class NewsDetailsVM
    {
        public int NewsId { get; set; }

        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string ImagePath { get; set; }

        public DateTime PublishDate { get; set; }

        public List<LatestNewsDisplayVM> RelatedNews
        {
            get;
            set;
        }

        public NewsDetailsVM()
        {
            RelatedNews =
                new List<LatestNewsDisplayVM>();
        }
    }
}