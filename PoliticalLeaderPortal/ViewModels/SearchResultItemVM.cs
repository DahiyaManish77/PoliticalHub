using System;

namespace PoliticalLeaderPortal.ViewModels
{
    public class SearchResultItemVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
        public string ImagePath { get; set; }
        public DateTime? PublishedDate { get; set; }
    }
}
