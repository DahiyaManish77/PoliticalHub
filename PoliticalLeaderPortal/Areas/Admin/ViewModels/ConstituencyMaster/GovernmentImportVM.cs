using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster
{
    public class GovernmentImportVM
    {
        public GovernmentImportVM()
        {
            SourceName = "LGD";
            UpdateExisting = true;
            StateOptions = new List<SelectListItem>();
            Errors = new List<string>();
        }

        [Required(ErrorMessage = "Please select a State / Union Territory.")]
        [Display(Name = "State / Union Territory")]
        public int? StateId { get; set; }

        [Display(Name = "Source")]
        [StringLength(100)]
        public string SourceName { get; set; }

        [Display(Name = "Update existing records having the same LGD code")]
        public bool UpdateExisting { get; set; }

        private HttpPostedFileBase _uploadedFile;

        [Display(Name = "Official LGD District XLSX")]
        public HttpPostedFileBase OfficialFile
        {
            get { return _uploadedFile; }
            set { _uploadedFile = value; }
        }

        public HttpPostedFileBase PackageFile
        {
            get { return _uploadedFile; }
            set { _uploadedFile = value; }
        }

        public HttpPostedFileBase UploadFile
        {
            get { return _uploadedFile; }
            set { _uploadedFile = value; }
        }

        public HttpPostedFileBase ImportFile
        {
            get { return _uploadedFile; }
            set { _uploadedFile = value; }
        }

        public HttpPostedFileBase File
        {
            get { return _uploadedFile; }
            set { _uploadedFile = value; }
        }

        public IList<SelectListItem> StateOptions { get; set; }

        public IList<SelectListItem> States
        {
            get { return StateOptions; }
            set { StateOptions = value ?? new List<SelectListItem>(); }
        }

        public GovernmentImportResultVM Result { get; set; }

        public GovernmentPackageImportResultVM PackageResult { get; set; }

        public IList<string> Errors { get; set; }
    }

    public class GovernmentImportResultVM
    {
        public GovernmentImportResultVM()
        {
            Errors = new List<string>();
            EntityType = "District";
        }

        public string FileName { get; set; }

        public string EntityType { get; set; }

        public string ReportStateCode { get; set; }

        public string ReportStateName { get; set; }

        public int TotalRows { get; set; }

        public int Inserted { get; set; }

        public int Updated { get; set; }

        public int Skipped { get; set; }

        public int Failed { get; set; }

        public IList<string> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return Failed > 0 ||
                       (Errors != null && Errors.Count > 0);
            }
        }

        public bool IsSuccessful
        {
            get { return !HasErrors; }
        }
    }

    public class GovernmentPackageImportResultVM
    {
        public GovernmentPackageImportResultVM()
        {
            Files = new List<GovernmentPackageFileResultVM>();
            Errors = new List<string>();
        }

        public string PackageName { get; set; }

        public int FilesDetected { get; set; }

        public int FilesImported { get; set; }

        public int FilesIgnored { get; set; }

        public int TotalRows { get; set; }

        public int Inserted { get; set; }

        public int Updated { get; set; }

        public int Skipped { get; set; }

        public int Failed { get; set; }

        public IList<GovernmentPackageFileResultVM> Files { get; set; }

        public IList<string> Errors { get; set; }
    }

    public class GovernmentPackageFileResultVM
    {
        public string FileName { get; set; }

        public string EntityType { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public int TotalRows { get; set; }

        public int Inserted { get; set; }

        public int Updated { get; set; }

        public int Skipped { get; set; }

        public int Failed { get; set; }
    }
}