using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class WebsiteSettingVM
    {
        public int WebsiteSettingId { get; set; }

        [Required]
        [Display(Name = "Website Name")]
        public string WebsiteName { get; set; }

        [Display(Name = "Website Tagline")]
        public string WebsiteTagline { get; set; }

        [Display(Name = "Default Meta Title")]
        public string DefaultMetaTitle { get; set; }

        [Display(Name = "Default Meta Description")]
        public string DefaultMetaDescription { get; set; }

        [Display(Name = "Default Meta Keywords")]
        public string DefaultMetaKeywords { get; set; }

        [Display(Name = "Google Analytics Code")]
        public string GoogleAnalyticsCode { get; set; }

        [Display(Name = "Google Search Console Code")]
        public string GoogleSearchConsoleCode { get; set; }

        [Display(Name = "Default Language")]
        public string DefaultLanguage { get; set; }

        public string WebsiteLogoPath { get; set; }

        public string FaviconPath { get; set; }

        public bool IsActive { get; set; }

        public HttpPostedFileBase LogoFile { get; set; }

        public HttpPostedFileBase FaviconFile { get; set; }
    }
}