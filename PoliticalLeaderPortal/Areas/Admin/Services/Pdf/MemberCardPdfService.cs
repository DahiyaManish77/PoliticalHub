using System;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Infrastructure.Pdf;
using QuestPDF.Fluent;

namespace PoliticalLeaderPortal.Areas.Admin.Services.Pdf
{
    public class MemberCardPdfService
    {
        public byte[] Generate(DigitalMemberCardVM model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var document = new DigitalMemberCardDocument(model);
            return document.GeneratePdf();
        }
    }
}
