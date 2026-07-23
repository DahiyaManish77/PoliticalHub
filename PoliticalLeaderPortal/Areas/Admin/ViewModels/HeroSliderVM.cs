using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class HeroSliderVM
    {
        public int HeroSliderId { get; set; }

        /*==========================================================
            CONTENT
        ==========================================================*/

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string Description { get; set; }

        /*==========================================================
            IMAGES (OLD)
        ==========================================================*/

        public string ImagePath { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        public string MobileImagePath { get; set; }

        public HttpPostedFileBase MobileImageFile { get; set; }

        /*==========================================================
            NEW IMAGES
        ==========================================================*/

        public string BackgroundImagePath { get; set; }

        public HttpPostedFileBase BackgroundImageFile { get; set; }

        public string LeaderImagePath { get; set; }

        public HttpPostedFileBase LeaderImageFile { get; set; }

        /*==========================================================
            VIDEO
        ==========================================================*/

        public bool IsVideoSlide { get; set; }

        public string VideoSourceType { get; set; }

        public string VideoUrl { get; set; }

        public string VideoPath { get; set; }

        public HttpPostedFileBase VideoFile { get; set; }

        public bool VideoAutoplay { get; set; }

        public bool VideoMuted { get; set; }

        public bool VideoLoop { get; set; }

        /*==========================================================
            BUTTONS
        ==========================================================*/

        public string ButtonText { get; set; }

        public string ButtonUrl { get; set; }

        public string ButtonText2 { get; set; }

        public string ButtonUrl2 { get; set; }

        public bool ShowButtons { get; set; }

        /*==========================================================
            DISPLAY
        ==========================================================*/

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public string TemplateType { get; set; }

        public string BackgroundColor { get; set; }

        public string HeroHeightCss { get; set; }

        public string LeaderImagePosition { get; set; }

        public bool ShowLeaderImage { get; set; }

        public bool ShowOverlay { get; set; }

        /*==========================================================
            ADVANCED (Keep for Backward Compatibility)
        ==========================================================*/

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
