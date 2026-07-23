using System;

namespace PoliticalLeaderPortal.ViewModels
{
    public class LatestNewsDisplayVM
    {
        public int NewsId { get; set; }


    public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string ImagePath { get; set; }

        public DateTime PublishDate { get; set; }

        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }
    }


}
