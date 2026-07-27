using System;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PoliticalLeaderPortal.Infrastructure.Pdf
{
    public class VerifiedLetterDocument : IDocument
    {
        private readonly VerifiedDocumentVM _model;
        private readonly string _verificationUrl;
        private readonly byte[] _qrCode;
        private readonly byte[] _leaderPhoto;
        private readonly byte[] _partyLogo;
        private readonly byte[] _memberPhoto;

        public VerifiedLetterDocument(VerifiedDocumentVM model, string verificationUrl)
        {
            _model = model;
            _verificationUrl = verificationUrl;
            PdfConfiguration.Initialize();
            PdfFontManager.Register();
            _qrCode = PdfImageHelper.LoadQrCode(QrCodeHelper.CreateBase64Png(verificationUrl));
            _leaderPhoto = PdfImageHelper.LoadImage("~/Content/images/leader.png");
            _partyLogo = PdfImageHelper.LoadImage("~/Content/images/bjp-lotus.png");
            _memberPhoto = PdfImageHelper.LoadMemberPhoto(model.RecipientPhotoPath);
        }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = _model.Subject ?? "Official verified letter",
                Author = "Sangeet Som Campaign Office",
                Subject = _model.DocumentNumber,
                Creator = "Sangeet Som Campaign Office"
            };
        }

        public DocumentSettings GetSettings()
        {
            return new DocumentSettings { ImageCompressionQuality = ImageCompressionQuality.High };
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(45);
                page.DefaultTextStyle(x => x.FontFamily(PdfFontManager.HindiFontFamily).FontSize(10).FontColor("#202124"));
                page.Header().Element(Header);
                page.Content().PaddingVertical(24).Element(Content);
                page.Footer().Element(Footer);
            });
        }

        private void Header(IContainer container)
        {
            container.BorderBottom(2).BorderColor("#0B6A3A").PaddingBottom(12).Row(row =>
            {
                row.ConstantItem(48).Height(48).PaddingRight(8).Element(LeaderPhoto);
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("SANGEET SOM").FontSize(16).Bold().FontColor("#0B6A3A");
                    column.Item().Text("CAMPAIGN OFFICE").FontSize(9).SemiBold().FontColor("#0B6A3A");
                    column.Item().Text("Official digitally verifiable communication").FontSize(8).FontColor("#667085");
                });
                row.ConstantItem(95).AlignRight().Column(column =>
                {
                    column.Item().Text(_model.DocumentNumber).FontSize(7).SemiBold();
                    column.Item().Text(_model.IssueDate.ToString("dd MMM yyyy")).FontSize(7);
                });
                row.ConstantItem(48).Height(48).PaddingLeft(8).Element(PartyLogo);
            });
        }

        private void LeaderPhoto(IContainer container)
        {
            if (_leaderPhoto == null)
                container.Border(1).BorderColor("#0B6A3A").AlignCenter().AlignMiddle().Text("SS").Bold();
            else
                container.Image(_leaderPhoto).FitArea();
        }

        private void PartyLogo(IContainer container)
        {
            if (_partyLogo == null)
                container.AlignCenter().AlignMiddle().Text("BJP").Bold().FontColor("#E85D04");
            else
                container.Image(_partyLogo).FitArea();
        }

        private void Content(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(13);
                column.Item().Text(GetTypeLabel()).FontSize(11).Bold().FontColor("#0B6A3A");
                column.Item().Text("To,").SemiBold();
                column.Item().Row(recipient =>
                {
                    recipient.RelativeItem().Column(to =>
                    {
                        to.Item().Text(_model.RecipientName).Bold();
                        if (!String.IsNullOrWhiteSpace(_model.RecipientRole)) to.Item().Text(_model.RecipientRole);
                        if (!String.IsNullOrWhiteSpace(_model.RecipientReference)) to.Item().Text("Reference: " + _model.RecipientReference);
                    });
                    recipient.ConstantItem(72).Height(84).Element(MemberPassportPhoto);
                });
                column.Item().Text(_model.Subject ?? GetTypeLabel()).FontSize(12).Bold();
                column.Item().Text(_model.BodyText ?? String.Empty).LineHeight(1.55f);
                column.Item().PaddingTop(15).Text("Sincerely,");
                column.Item().Column(signature =>
                {
                    signature.Item().Text(_model.IssuedByName ?? "Authorized issuer").Bold();
                    signature.Item().Text(_model.IssuedByDesignation ?? "Campaign Office");
                });
                column.Item().PaddingTop(15).Border(1).BorderColor("#D0D5DD").Background("#F8FAFC").Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(verify =>
                    {
                        verify.Item().Text("DOCUMENT VERIFICATION").FontSize(9).Bold().FontColor("#0B6A3A");
                        verify.Item().Text("Scan this QR code or use the official URL to confirm current validity.")
                            .FontSize(8);
                        verify.Item().Text(_verificationUrl).FontSize(7).FontColor("#0B6A3A");
                    });
                    row.ConstantItem(62).Height(62).Image(_qrCode).FitArea();
                });
            });
        }

        private void MemberPassportPhoto(IContainer container)
        {
            if (_memberPhoto == null)
                container.Border(1).BorderColor("#667085").Background("#F8FAFC")
                    .AlignCenter().AlignMiddle().Text("MEMBER\nPHOTO").FontSize(8).SemiBold().FontColor("#667085");
            else
                container.Border(1).BorderColor("#667085").Padding(1).Image(_memberPhoto).FitArea();
        }

        private void Footer(IContainer container)
        {
            container.BorderTop(1).BorderColor("#D0D5DD").PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text("Generated by Sangeet Som Campaign Office").FontSize(7).FontColor("#667085");
                row.RelativeItem().AlignRight().Text("Page 1 of 1").FontSize(7).FontColor("#667085");
            });
        }

        private string GetTypeLabel()
        {
            switch (_model.DocumentType)
            {
                case "AppointmentLetter": return "APPOINTMENT LETTER";
                case "AuthorizationLetter": return "AUTHORIZATION LETTER";
                case "VolunteerLetter": return "VOLUNTEER CONFIRMATION LETTER";
                default: return "OFFICIAL LETTER";
            }
        }
    }
}
