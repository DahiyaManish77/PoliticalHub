using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliticalLeaderPortal.ViewModels
{
    public class SearchPageVM
    {
        public string Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<SearchResultItemVM> Results { get; set; }

        public SearchPageVM()
        {
            Results = new List<SearchResultItemVM>();
        }

        public int TotalResults
        {
            get { return Results == null ? 0 : Results.Count; }
        }

        public IEnumerable<IGrouping<string, SearchResultItemVM>> GroupedResults
        {
            get
            {
                return (Results ?? new List<SearchResultItemVM>())
                    .GroupBy(x => x.Category)
                    .OrderBy(x => x.Key);
            }
        }
    }
}
