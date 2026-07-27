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
        private const string SoftCream = "#FFF8ED";
        private const string SoftGray = "#F2F2EE";
        private const string Orange = "#F26B1D";
        private const string Charcoal = "#171916";

        private const float HeaderHeight = 34f;
        private const float FooterHeight = 16f;
        private const float CardPadding = 8f;
        private const float PhotoWidth = 58f;
        private const float PhotoHeight = 58f;
        private const float QrSize = 25f;

        private readonly DigitalMemberCardVM _model;
        private readonly byte[] _leaderPhoto;
        private readonly byte[] _partyLogo;
        private readonly byte[] _memberPhoto;
        private readonly byte[] _qrCode;

        public DigitalMemberCardDocument(DigitalMemberCardVM model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            _model = model;
            PdfConfiguration.Initialize();
            PdfFontManager.Register();

            _leaderPhoto = PdfImageHelper.LoadImage(model.LeaderPhotoPath ?? "~/Content/images/leader.png");
            _partyLogo = PdfImageHelper.LoadImage(model.PartyLogoPath ?? "~/Content/images/bjp-lotus.png");
            _memberPhoto = PdfImageHelper.LoadMemberPhoto(model.PhotoPath);
            _qrCode = PdfImageHelper.LoadQrCode(model.QrCodeBase64);
        }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = "Digital Member PVC Card",
                Author = "Sangeet Som Campaign Office",
                Subject = "Ready-to-print PVC membership card",
                Creator = "Sangeet Som Campaign Office",
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
            container.Border(1).BorderColor("#DDD8CF").Background(SoftCream).Layers(layers =>
            {
                layers.Layer().AlignRight().Width(78).Background(PrimaryGreen);
                layers.PrimaryLayer().Padding(9).Row(row =>
                {
                    row.ConstantItem(70).Element(ComposeModernIdentityPanel);
                    row.RelativeItem().PaddingLeft(10).Element(ComposeModernContactPanel);
                });
            });
        }

        private void ComposeModernIdentityPanel(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(4);
                column.Item().Height(78).Element(ComposeModernPhoto);
                column.Item().AlignCenter().Text(Clean(_model.PartyMemberCode))
                    .FontSize(5.2f).Bold().FontColor(PrimaryGreen);
                column.Item().AlignCenter().Text("VERIFIED MEMBER")
                    .FontSize(4.3f).SemiBold().FontColor(Orange);
            });
        }

        private void ComposeModernPhoto(IContainer container)
        {
            if (_memberPhoto == null)
            {
                container.Border(1).BorderColor("#D9D4CA").Background(SoftGray).AlignCenter().AlignMiddle()
                    .Text("MEMBER\nPHOTO").FontSize(7).Bold().FontColor(Gray);
                return;
            }

            container.Border(2).BorderColor(White).Background(White).Padding(2).Image(_memberPhoto).FitArea();
        }

        private void ComposeModernContactPanel(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(3);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(brand =>
                    {
                        brand.Item().Text("SANGEET SOM").FontSize(5).SemiBold().FontColor(Orange);
                        brand.Item().Text("DIGITAL MEMBER CARD").FontSize(3.8f).FontColor(Gray);
                    });
                    row.ConstantItem(20).Height(20).Element(ComposePartyLogo);
                });
                column.Item().PaddingTop(2).Text(Clean(_model.FullName))
                    .FontSize(13).ExtraBold().FontColor(Charcoal);
                column.Item().Text(BuildRoleLine()).FontSize(5.2f).SemiBold().FontColor(PrimaryGreen);
                column.Item().PaddingTop(2).LineHorizontal(0.7f).LineColor("#D7D1C7");
                column.Item().Element(x => ComposeContactRow(x, "P", Clean(_model.Phone)));
                column.Item().Element(x => ComposeContactRow(x, "A", Clean(_model.FullAddress)));
                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(ComposeSocialButtons);
                    row.ConstantItem(29).Height(29).Element(ComposeQrCode);
                });
            });
        }

        private void ComposeContactRow(IContainer container, string symbol, string value)
        {
            container.MinHeight(18).Row(row =>
            {
                row.ConstantItem(16).Height(16).Background(Orange).AlignCenter().AlignMiddle()
                    .Text(symbol).FontSize(5.5f).Bold().FontColor(White);
                row.RelativeItem().PaddingLeft(5).AlignMiddle().Text(value)
                    .FontSize(symbol == "A" ? 4.7f : 5.3f).FontColor(Charcoal);
            });
        }

        private void ComposeSocialButtons(IContainer container)
        {
            container.AlignMiddle().Row(row =>
            {
                AddSocialButton(row, "f", _model.FacebookUrl, "#1877F2");
                AddSocialButton(row, "ig", _model.InstagramUrl, "#C13584");
                AddSocialButton(row, "X", _model.TwitterUrl, Charcoal);
                AddSocialButton(row, "▶", _model.YoutubeUrl, "#E62117");
            });
        }

        private void AddSocialButton(RowDescriptor row, string symbol, string url, string color)
        {
            row.ConstantItem(18).PaddingRight(3).Column(column =>
            {
                column.Item().Height(15).Background(String.IsNullOrWhiteSpace(url) ? "#B8B8B3" : color)
                    .AlignCenter().AlignMiddle().Text(symbol).FontSize(symbol == "ig" ? 4.2f : 5.2f).Bold().FontColor(White);
            });
        }

        private string BuildRoleLine()
        {
            string designation = String.IsNullOrWhiteSpace(_model.Designation) ? "Member" : _model.Designation.Trim();
            return String.IsNullOrWhiteSpace(_model.WingName)
                ? designation
                : designation + "  •  " + _model.WingName.Trim();
        }

        private void ComposeFrontHeader(IContainer container)
        {
            container.Background("#FFF8F1").BorderBottom(2).BorderColor(PrimaryGreen).Padding(3).Row(row =>
            {
                row.ConstantItem(27).Element(ComposeLeaderBrandPhoto);
                row.RelativeItem().PaddingHorizontal(5).AlignMiddle().Column(column =>
                {
                    column.Item().AlignCenter().Text("SANGEET SOM").FontSize(10).Bold().FontColor(DarkGreen);
                    column.Item().AlignCenter().Text("OFFICIAL CAMPAIGN IDENTITY CARD").FontSize(5).SemiBold().FontColor(Red);
                });
                row.ConstantItem(27).Element(ComposePartyLogo);
            });
        }

        private void ComposeLeaderBrandPhoto(IContainer container)
        {
            if (_leaderPhoto == null)
            {
                container.Border(1).BorderColor(PrimaryGreen).AlignCenter().AlignMiddle().Text("SS").FontSize(6).Bold();
                return;
            }
            container.Border(0.7f).BorderColor(PrimaryGreen).Image(_leaderPhoto).FitArea();
        }

        private void ComposePartyLogo(IContainer container)
        {
            if (_partyLogo == null)
            {
                container.AlignCenter().AlignMiddle().Text("BJP").FontSize(6).Bold().FontColor(Red);
                return;
            }
            container.Image(_partyLogo).FitArea();
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
            });
        }

        private void ComposeMemberPhoto(IContainer container)
        {
            if (_memberPhoto == null)
            {
                container.Border(1).BorderColor(Gray).Background("#F5F5F5").AlignCenter().AlignMiddle()
                    .Text("MEMBER\nPHOTO").FontSize(6).SemiBold().FontColor(Gray);
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
            container.Border(1).BorderColor(PrimaryGreen).Background(White).Padding(5).Column(column =>
            {
                column.Spacing(2);
                column.Item().Element(ComposeApprover);
                column.Item().Element(DrawSeparator);
                column.Item().Element(ComposeVerification);
                column.Item().Element(DrawSeparator);
                column.Item().Element(ComposeTerms);
            });
        }

        private void ComposeApprover(IContainer container)
        {
            container.Border(1).BorderColor(PrimaryGreen).Background("#F8FFF8").Padding(3).Column(column =>
            {
                column.Spacing(1);
                column.Item().AlignCenter().Text("Approved By").FontSize(7).Bold().FontColor(PrimaryGreen);
                AddIssuerField(column, "Name", Clean(_model.ApprovedByName));
                AddIssuerField(column, "Designation", Clean(_model.ApprovedByDesignation));
                AddIssuerField(column, "Wing", Clean(_model.ApprovedByWingName));
                AddIssuerField(column, "Phone", Clean(_model.ApprovedByPhone));
            });
        }

        private void ComposeVerification(IContainer container)
        {
            container.Border(1).BorderColor(DarkGreen).Background("#FCFFFC").Padding(3).Column(column =>
            {
                column.Spacing(1);
                column.Item().AlignCenter().Text("Verification").FontSize(6.5f).Bold().FontColor(PrimaryGreen);
                column.Item().Text("Scan the QR code or verify this member card on the official portal. This card is valid only for the named member.")
                    .FontSize(4.3f).FontColor(Black);
                column.Item().AlignCenter().Text(Clean(_model.VerificationUrl)).FontSize(4f).SemiBold().FontColor(PrimaryGreen);
            });
        }

        private void ComposeTerms(IContainer container)
        {
            container.Border(1).BorderColor(Gray).Background("#FFFDF8").Padding(3).Column(column =>
            {
                column.Spacing(0.5f);
                column.Item().AlignCenter().Text("Important Instructions").FontSize(5.5f).Bold().FontColor(Red);
                column.Item().Text("- Organization property; report loss immediately.").FontSize(4.2f);
                column.Item().Text("- Use by any other person is prohibited.").FontSize(4.2f);
                column.Item().AlignRight().Text("Authorized Signature").FontSize(4.8f).SemiBold().FontColor(PrimaryGreen);
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
                row.ConstantItem(44).Text(label).FontSize(4.8f).SemiBold().FontColor(PrimaryGreen);
                row.ConstantItem(5).AlignCenter().Text(":").FontSize(4.8f);
                row.RelativeItem().Text(Clean(value)).FontSize(4.8f).FontColor(Black);
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
