using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class DigitalMemberCardVM
    {
        public string PartyMemberCode { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public string Phone { get; set; }
        public string Designation { get; set; }
        public string WingName { get; set; }
        public string FullAddress { get; set; }
        public DateTime? ValidTill { get; set; }

        public string PhotoPath { get; set; }
        public string LogoPath { get; set; }
        public string LeaderPhotoPath { get; set; }
        public string PartyLogoPath { get; set; }
        public string QrCodeBase64 { get; set; }
        public string VerificationUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string YoutubeUrl { get; set; }

        public string ApprovedByName { get; set; }
        public string ApprovedByDesignation { get; set; }
        public string ApprovedByWingName { get; set; }
        public string ApprovedByPhone { get; set; }
    }
}
