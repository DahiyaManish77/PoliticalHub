using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class StatisticVM
    {
        public int StatisticId { get; set; }


    public string Title { get; set; }

        public long StatisticValue { get; set; }

        public string IconClass { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }


}
