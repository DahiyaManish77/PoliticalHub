using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster
{
    public class GeographyDashboardVM
    {
        public int States { get; set; }
        public int Districts { get; set; }
        public int Tehsils { get; set; }
        public int Blocks { get; set; }
        public int ParliamentaryConstituencies { get; set; }
        public int AssemblyConstituencies { get; set; }
        public int Wards { get; set; }
        public int GramPanchayats { get; set; }
        public int Villages { get; set; }
        public string EntityType { get; set; }
        public string Keyword { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }
        public int TotalPages { get; set; }
        public IList<GeographyRowVM> Rows { get; set; } = new List<GeographyRowVM>();
    }

    public class GeographyRowVM
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameEnglish { get; set; }
        public string NameHindi { get; set; }
        public string ParentName { get; set; }
        public string EntityType { get; set; }
        public bool IsActive { get; set; }
    }

    public class GeographyEditVM
    {
        public int Id { get; set; }
        [Required] public string EntityType { get; set; }
        [Required, StringLength(20)] public string Code { get; set; }
        [Required, StringLength(200)] public string NameEnglish { get; set; }
        [StringLength(200)] public string NameHindi { get; set; }
        public int? StateId { get; set; }
        public int? DistrictId { get; set; }
        public int? TehsilId { get; set; }
        public int? BlockId { get; set; }
        public int? ParliamentaryConstituencyId { get; set; }
        public int? AssemblyConstituencyId { get; set; }
        public int? GramPanchayatId { get; set; }
        [StringLength(30)] public string ReservationCategory { get; set; }
        [StringLength(30)] public string AreaType { get; set; }
        public bool IsActive { get; set; } = true;
        public IEnumerable<SelectListItem> States { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Districts { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Tehsils { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Blocks { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ParliamentaryConstituencies { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AssemblyConstituencies { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> GramPanchayats { get; set; } = new List<SelectListItem>();
    }

    public class GeographyImportVM
    {
        [Required] public string EntityType { get; set; }
        public bool UpdateExisting { get; set; } = true;
        public string SourceName { get; set; } = "LGD / ECI";
    }
}
