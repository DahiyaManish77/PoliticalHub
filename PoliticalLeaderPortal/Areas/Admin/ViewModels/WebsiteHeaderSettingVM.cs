using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class WebsiteHeaderSettingVM
    {
        public int WebsiteHeaderSettingId { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public string Address { get; set; }

        public string FacebookUrl { get; set; }

        public string InstagramUrl { get; set; }

        public string TwitterUrl { get; set; }

        public string YoutubeUrl { get; set; }

        public string WhatsappUrl { get; set; }

        public bool ShowTopBar { get; set; }

        public bool ShowLanguageSwitcher { get; set; }

        public bool ShowSignIn { get; set; }

        public bool ShowSignUp { get; set; }

        public bool IsActive { get; set; }

        public string LogoPath { get; set; }

        public string LeaderImagePath { get; set; }

        public string HeaderBackgroundColor { get; set; }

        public string HeaderTextColor { get; set; }

        public string HeaderFontFamily { get; set; }

        public string HeaderFontSize { get; set; }

        public string LogoAnimationClass { get; set; }

        public string TextAnimationClass { get; set; }

        public HttpPostedFileBase LogoFile { get; set; }

        public HttpPostedFileBase LeaderImageFile { get; set; }
    }
}