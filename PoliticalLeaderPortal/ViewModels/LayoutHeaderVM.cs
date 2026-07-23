namespace PoliticalLeaderPortal.ViewModels
{
    public class LayoutHeaderVM
    {
        public string WebsiteName { get; set; }

        public string WebsiteTagline { get; set; }

        public string LogoPath { get; set; }

        public string LeaderImagePath { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public bool ShowTopBar { get; set; }

        public bool ShowLanguageSwitcher { get; set; }

        public bool ShowSignIn { get; set; }

        public bool ShowSignUp { get; set; }

        public string HeaderBackgroundColor { get; set; }

        public string HeaderTextColor { get; set; }

        public string HeaderFontFamily { get; set; }

        public string HeaderFontSize { get; set; }
    }
}