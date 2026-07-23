using System;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PoliticalLeaderPortal.Infrastructure.Pdf
{
    public class DigitalMemberCardDocument : IDocument
    {
        private const float CardWidth = 242.65f;  // 85.60 mm, CR80 PVC card
        private const float CardHeight = 153.00f; // 53.98 mm, CR80 PVC card

        private const string PrimaryGreen = "#0B7A39";
        private const string DarkGreen = "#055628";
        private const string Red = "#D32F2F";
        private const string Gold = "#D4AF37";
        private const string White = "#FFFFFF";
        private const string Black = "#222222";
        private const string Gray = "#666666";

        private const float HeaderHeight = 34f;
        private const float FooterHeight = 16f;
        private const float CardPadding = 8f;
        private const float PhotoWidth = 58f;
        private const float PhotoHeight = 72f;
        private const float QrSize = 30f;

        private readonly DigitalMemberCardVM _model;
        private readonly byte[] _logo;
        private readonly byte[] _memberPhoto;
        private readonly byte[] _qrCode;

        public DigitalMemberCardDocument(DigitalMemberCardVM model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            _model = model;
            PdfConfiguration.Initialize();
            PdfFontManager.Register();

            _logo = PdfImageHelper.LoadImage(model.LogoPath) ?? PdfImageHelper.LoadLogo();
            _memberPhoto = PdfImageHelper.LoadMemberPhoto(model.PhotoPath);
            _qrCode = PdfImageHelper.LoadQrCode(model.QrCodeBase64);
        }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = "Digital Member PVC Card",
                Author = "Political Leader Portal",
                Subject = "Ready-to-print PVC membership card",
                Creator = "Political Leader Portal",
                Keywords = "membership, pvc card, qr code"
            };
        }

        public DocumentSettings GetSettings()
        {
            return new DocumentSettings
            {
                ImageCompressionQuality = ImageCompressionQuality.High,
                ContentDirection = ContentDirection.LeftToRight,
                PdfA = false
            };
        }

        public void Compose(IDocumentContainer container)
        {
            ComposeFrontPage(container);
            ComposeBackPage(container);
        }

        private void ComposeFrontPage(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(CardWidth, CardHeight);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily(PdfFontManager.HindiFontFamily).FontSize(7).FontColor(Black));
                page.Content().Element(ComposeFrontCard);
            });
        }

        private void ComposeBackPage(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(CardWidth, CardHeight);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily(PdfFontManager.HindiFontFamily).FontSize(7).FontColor(Black));
                page.Content().Element(ComposeBackCard);
            });
        }

        private void ComposeFrontCard(IContainer container)
        {
            container.Border(1).BorderColor(PrimaryGreen).Background(White).Column(column =>
            {
                column.Item().Height(HeaderHeight).Element(ComposeFrontHeader);
                column.Item().Height(CardHeight - HeaderHeight - FooterHeight).Padding(CardPadding).Row(row =>
                {
                    row.RelativeItem().PaddingRight(6).Element(ComposeMemberDetails);
                    row.ConstantItem(PhotoWidth + 8).Element(ComposeRightPanel);
                });
                column.Item().Height(FooterHeight).Background(PrimaryGreen).PaddingHorizontal(8).AlignMiddle()
                    .Text("Digital Membership Card").FontSize(6).SemiBold().FontColor(White);
            });
        }

        private void ComposeFrontHeader(IContainer container)
        {
            container.Background(PrimaryGreen).Padding(5).Row(row =>
            {
                row.ConstantItem(25).Element(ComposeLogo);
                row.RelativeItem().PaddingLeft(5).Column(column =>
                {
                    column.Item().Text("MEMBER PVC CARD").FontSize(12).Bold().FontColor(White);
                    column.Item().Text("Ready to Print - CR80 Size 85.60mm x 53.98mm").FontSize(5).FontColor(Gold);
                });
            });
        }

        private void ComposeLogo(IContainer container)
        {
            if (_logo == null)
            {
                container.Border(1).BorderColor(White).AlignCenter().AlignMiddle().Text("LOGO").FontSize(5).FontColor(White);
                return;
            }

            container.Image(_logo).FitArea();
        }

        private void ComposeMemberDetails(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(2);
                AddField(column, "Member ID", Clean(_model.PartyMemberCode));
                AddField(column, "Name", Clean(_model.FullName));
                AddField(column, "Father", Clean(_model.FatherName));
                AddField(column, "Designation", Clean(_model.Designation));
                AddField(column, "Wing", Clean(_model.WingName));
                AddField(column, "Phone", Clean(_model.Phone));
                AddField(column, "Valid Till", FormatDate(_model.ValidTill));

                if (!String.IsNullOrWhiteSpace(_model.FullAddress))
                    AddAddress(column, _model.FullAddress);
            });
        }

        private void ComposeRightPanel(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(4);
                column.Item().Height(PhotoHeight).Element(ComposeMemberPhoto);
                column.Item().AlignCenter().Width(QrSize).Height(QrSize).Element(ComposeQrCode);
                column.Item().AlignCenter().Text(Clean(_model.PartyMemberCode)).FontSize(5).SemiBold().FontColor(PrimaryGreen);
            });
        }

        private void ComposeMemberPhoto(IContainer container)
        {
            if (_memberPhoto == null)
            {
                container.Border(1).BorderColor(Gray).Background("#F5F5F5").AlignCenter().AlignMiddle()
                    .Text("PHOTO").FontSize(7).FontColor(Gray);
                return;
            }

            container.Border(1).BorderColor(Gray).Padding(1).Image(_memberPhoto).FitArea();
        }

        private void ComposeQrCode(IContainer container)
        {
            if (_qrCode == null)
            {
                container.Border(1).BorderColor(Gray).AlignCenter().AlignMiddle().Text("QR").FontSize(7).FontColor(Gray);
                return;
            }

            container.Image(_qrCode).FitArea();
        }

        private void ComposeBackCard(IContainer container)
        {
            container.Border(1).BorderColor(PrimaryGreen).Background(White).Padding(8).Column(column =>
            {
                column.Spacing(5);
                column.Item().Element(ComposeApprover);
                column.Item().Element(DrawSeparator);
                column.Item().Element(ComposeVerification);
                column.Item().Element(DrawSeparator);
                column.Item().Element(ComposeTerms);
            });
        }

        private void ComposeApprover(IContainer container)
        {
            container.Border(1).BorderColor(PrimaryGreen).Background("#F8FFF8").Padding(5).Column(column =>
            {
                column.Spacing(2);
                column.Item().AlignCenter().Text("Approved By").FontSize(9).Bold().FontColor(PrimaryGreen);
                AddIssuerField(column, "Name", Clean(_model.ApprovedByName));
                AddIssuerField(column, "Designation", Clean(_model.ApprovedByDesignation));
                AddIssuerField(column, "Wing", Clean(_model.ApprovedByWingName));
                AddIssuerField(column, "Phone", Clean(_model.ApprovedByPhone));
            });
        }

        private void ComposeVerification(IContainer container)
        {
            container.Border(1).BorderColor(DarkGreen).Background("#FCFFFC").Padding(5).Column(column =>
            {
                column.Spacing(2);
                column.Item().AlignCenter().Text("Verification").FontSize(8).Bold().FontColor(PrimaryGreen);
                column.Item().Text("Scan the QR code or verify this member card on the official portal. This card is valid only for the named member.")
                    .FontSize(5.5f).FontColor(Black);
                column.Item().AlignCenter().Text(Clean(_model.VerificationUrl)).FontSize(5.5f).SemiBold().FontColor(PrimaryGreen);
            });
        }

        private void ComposeTerms(IContainer container)
        {
            container.Border(1).BorderColor(Gray).Background("#FFFDF8").Padding(5).Column(column =>
            {
                column.Spacing(1);
                column.Item().AlignCenter().Text("Important Instructions").FontSize(7).Bold().FontColor(Red);
                column.Item().Text("- This card is organization property.").FontSize(5);
                column.Item().Text("- If lost, inform the issuing office immediately.").FontSize(5);
                column.Item().Text("- Misuse by any other person is prohibited.").FontSize(5);
                column.Item().AlignRight().Text("Authorized Signature").FontSize(6).SemiBold().FontColor(PrimaryGreen);
            });
        }

        private void AddField(ColumnDescriptor column, string label, string value)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(42).Text(label).FontSize(6).SemiBold().FontColor(PrimaryGreen);
                row.ConstantItem(5).AlignCenter().Text(":").FontSize(6).SemiBold();
                row.RelativeItem().Text(Clean(value)).FontSize(6).FontColor(Black);
            });
        }

        private void AddAddress(ColumnDescriptor column, string address)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(42).Text("Address").FontSize(6).SemiBold().FontColor(PrimaryGreen);
                row.ConstantItem(5).AlignTop().Text(":").FontSize(6).SemiBold();
                row.RelativeItem().Text(Clean(address)).FontSize(5.5f).FontColor(Black);
            });
        }

        private void AddIssuerField(ColumnDescriptor column, string label, string value)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(44).Text(label).FontSize(5.5f).SemiBold().FontColor(PrimaryGreen);
                row.ConstantItem(5).AlignCenter().Text(":").FontSize(5.5f);
                row.RelativeItem().Text(Clean(value)).FontSize(5.5f).FontColor(Black);
            });
        }

        private void DrawSeparator(IContainer container)
        {
            container.LineHorizontal(0.8f).LineColor(PrimaryGreen);
        }

        private static string Clean(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private static string FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd-MM-yyyy") : "-";
        }
    }
}
