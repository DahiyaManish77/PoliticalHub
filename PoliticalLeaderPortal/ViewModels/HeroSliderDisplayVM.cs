namespace PoliticalLeaderPortal.ViewModels
{
    public class HeroSliderDisplayVM
    {
        public int HeroSliderId { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string Description { get; set; }

        // Existing Images
        public string ImagePath { get; set; }

        public string MobileImagePath { get; set; }

        // New Images
        public string BackgroundImagePath { get; set; }

        public string LeaderImagePath { get; set; }

        public string AssetVersion { get; set; }

        // Video
        public bool IsVideoSlide { get; set; }

        public string VideoSourceType { get; set; }

        public string VideoUrl { get; set; }

        public string VideoPath { get; set; }

        public bool VideoAutoplay { get; set; }

        public bool VideoMuted { get; set; }

        public bool VideoLoop { get; set; }

        // Appearance
        public string BackgroundColor { get; set; }

        public string HeroHeightCss { get; set; }

        public string LeaderImagePosition { get; set; }

        public bool ShowLeaderImage { get; set; }

        public bool ShowOverlay { get; set; }

        public bool ShowButtons { get; set; }

        // Buttons
        public string ButtonText { get; set; }

        public string ButtonUrl { get; set; }

        public string ButtonText2 { get; set; }

        public string ButtonUrl2 { get; set; }

        // Display
        public int DisplayOrder { get; set; }

        public string TemplateType { get; set; }

        // Existing (Keep for Compatibility)
        public string OverlayType { get; set; }

        public string SliderTransition { get; set; }

        public string TitleAnimation { get; set; }

        public string SubTitleAnimation { get; set; }

        public string DescriptionAnimation { get; set; }

        public string ButtonAnimation { get; set; }

        public string TextAlignment { get; set; }

        public decimal? OverlayOpacity { get; set; }
    }
}
