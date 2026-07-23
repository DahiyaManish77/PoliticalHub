using System;
using System.Collections.Generic;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class HomePageSectionPageVM
    {
        public HomePageSectionPageVM()
        {
            Sections = new List<HomePageSectionVM>();
        }

        public IList<HomePageSectionVM> Sections { get; set; }
    }

    public class HomePageSectionVM
    {
        public int HomePageSectionId { get; set; }
        public string SectionKey { get; set; }
        public string SectionName { get; set; }
        public string Description { get; set; }
        public string RenderType { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string PartialViewPath { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSystem { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
