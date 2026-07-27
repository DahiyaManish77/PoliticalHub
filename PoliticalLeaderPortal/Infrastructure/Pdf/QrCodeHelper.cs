using System;
using QRCoder;

namespace PoliticalLeaderPortal.Infrastructure.Pdf
{
    public static class QrCodeHelper
    {
        public static string CreateBase64Png(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            return "data:image/png;base64," + Convert.ToBase64String(CreatePngBytes(value));
        }

        public static byte[] CreatePngBytes(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return new byte[0];

            using (var generator = new QRCodeGenerator())
            using (var data = generator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q))
            {
                var qr = new PngByteQRCode(data);
                return qr.GetGraphic(12);
            }
        }
    }
}
