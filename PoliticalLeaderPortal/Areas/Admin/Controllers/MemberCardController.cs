using System;
using System.Web.Mvc;
using PoliticalLeaderPortal.Areas.Admin.Services.Pdf;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class MemberCardController : Controller
    {
        private readonly MemberCardPdfService _pdfService;

        public MemberCardController()
        {
            _pdfService = new MemberCardPdfService();
        }

        public ActionResult Download()
        {
            var model = new DigitalMemberCardVM
            {
                PartyMemberCode = Request["memberCode"],
                FullName = Request["name"],
                FatherName = Request["fatherName"],
                Phone = Request["phone"],
                Designation = Request["designation"],
                WingName = Request["wingName"],
                FullAddress = Request["address"],
                PhotoPath = Request["photoPath"],
                LogoPath = Request["logoPath"],
                QrCodeBase64 = Request["qrCodeBase64"],
                VerificationUrl = Request["verificationUrl"],
                ApprovedByName = Request["approvedByName"],
                ApprovedByDesignation = Request["approvedByDesignation"],
                ApprovedByWingName = Request["approvedByWingName"],
                ApprovedByPhone = Request["approvedByPhone"],
                ValidTill = DateTime.Today.AddYears(1)
            };

            byte[] pdf = _pdfService.Generate(model);
            string fileName = "member-pvc-card-" + SafeFilePart(model.PartyMemberCode) + ".pdf";

            return File(pdf, "application/pdf", fileName);
        }

        private static string SafeFilePart(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return DateTime.Now.ToString("yyyyMMddHHmmss");

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(c.ToString(), String.Empty);

            return value.Trim();
        }
    }
}
