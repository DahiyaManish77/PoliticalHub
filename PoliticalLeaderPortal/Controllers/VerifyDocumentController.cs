using System;
using System.Web.Mvc;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;

namespace PoliticalLeaderPortal.Controllers
{
    public class VerifyDocumentController : Controller
    {
        private readonly VerifiedDocumentService _service = new VerifiedDocumentService();

        public ActionResult Index(string code)
        {
            var document = String.IsNullOrWhiteSpace(code) ? null : _service.GetByCode(code.Trim());
            if (document == null)
                return View(new PublicDocumentVerificationVM
                {
                    Found = false,
                    IsValid = false,
                    VerificationState = "Not found"
                });

            bool expired = document.ExpiryDate.HasValue && document.ExpiryDate.Value.Date < DateTime.Today;
            bool valid = document.Status == "Active" && !expired;
            return View(new PublicDocumentVerificationVM
            {
                Found = true,
                IsValid = valid,
                VerificationState = document.Status == "Revoked" ? "Revoked" : (expired ? "Expired" : "Valid"),
                DocumentNumber = document.DocumentNumber,
                DocumentType = document.DocumentType,
                RecipientName = document.RecipientName,
                RecipientReference = document.RecipientReference,
                RecipientRole = document.RecipientRole,
                CampaignName = document.CampaignName,
                Subject = document.Subject,
                IssueDate = document.IssueDate,
                ExpiryDate = document.ExpiryDate,
                IssuedByName = document.IssuedByName,
                IssuedByDesignation = document.IssuedByDesignation
            });
        }
    }
}
