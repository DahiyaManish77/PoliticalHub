using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Infrastructure.Uploads
{
    public static class SecureUploadValidator
    {
        private static readonly HashSet<string> ImageExtensions =
            new HashSet<string>(new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }, StringComparer.OrdinalIgnoreCase);

        public static string ValidateImage(HttpPostedFileBase file, int maximumBytes, bool allowIcon)
        {
            ValidatePresentFile(file, maximumBytes, "image");
            string extension = NormalizeExtension(file.FileName);
            bool validExtension = ImageExtensions.Contains(extension) ||
                                  (allowIcon && extension == ".ico");

            if (!validExtension)
                throw new InvalidOperationException("Only JPG, PNG, GIF, WEBP" + (allowIcon ? " or ICO" : "") + " images are allowed.");

            if (String.IsNullOrWhiteSpace(file.ContentType) ||
                (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
                 !(allowIcon && file.ContentType == "application/octet-stream")))
                throw new InvalidOperationException("The uploaded file is not a recognized image.");

            if (!MatchesImageSignature(ReadHeader(file, 16), extension))
                throw new InvalidOperationException("The image contents do not match its file extension.");

            return extension;
        }

        public static string ValidatePdf(HttpPostedFileBase file, int maximumBytes)
        {
            ValidatePresentFile(file, maximumBytes, "PDF");
            string extension = NormalizeExtension(file.FileName);
            byte[] header = ReadHeader(file, 5);

            if (extension != ".pdf" || header.Length < 5 ||
                header[0] != 0x25 || header[1] != 0x50 || header[2] != 0x44 ||
                header[3] != 0x46 || header[4] != 0x2D)
                throw new InvalidOperationException("The uploaded file is not a valid PDF document.");

            return extension;
        }

        private static void ValidatePresentFile(HttpPostedFileBase file, int maximumBytes, string label)
        {
            if (file == null || file.ContentLength <= 0)
                throw new InvalidOperationException("Select a valid " + label + " file.");

            if (file.ContentLength > maximumBytes)
                throw new InvalidOperationException("The " + label + " file must be smaller than " +
                    Math.Max(1, maximumBytes / (1024 * 1024)) + " MB.");
        }

        private static string NormalizeExtension(string fileName)
        {
            return (Path.GetExtension(Path.GetFileName(fileName)) ?? String.Empty).ToLowerInvariant();
        }

        private static byte[] ReadHeader(HttpPostedFileBase file, int length)
        {
            Stream stream = file.InputStream;
            long position = stream.CanSeek ? stream.Position : 0;
            byte[] buffer = new byte[length];
            int count = stream.Read(buffer, 0, buffer.Length);
            if (stream.CanSeek) stream.Position = position;
            return buffer.Take(count).ToArray();
        }

        private static bool MatchesImageSignature(byte[] h, string extension)
        {
            if (h.Length < 4) return false;
            if (extension == ".jpg" || extension == ".jpeg")
                return h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
            if (extension == ".png")
                return h.Length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E &&
                       h[3] == 0x47 && h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A;
            if (extension == ".gif")
                return h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x38;
            if (extension == ".webp")
                return h.Length >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 &&
                       h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50;
            if (extension == ".ico")
                return h[0] == 0x00 && h[1] == 0x00 && h[2] == 0x01 && h[3] == 0x00;
            return false;
        }
    }
}
