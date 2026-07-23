using System;
using System.IO;
using System.Web;

namespace PoliticalLeaderPortal.Infrastructure.Pdf
{
    public static class PdfImageHelper
    {
        public static byte[] LoadLogo()
        {
            return LoadImage("~/Content/images/logo.png");
        }

        public static byte[] LoadMemberPhoto(string path)
        {
            return LoadImage(path);
        }

        public static byte[] LoadQrCode(string base64)
        {
            if (String.IsNullOrWhiteSpace(base64))
                return null;

            string value = base64.Trim();
            int commaIndex = value.IndexOf(',');

            if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
                value = value.Substring(commaIndex + 1);

            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static byte[] LoadImage(string path)
        {
            string physicalPath = ResolvePath(path);

            if (String.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
                return null;

            return File.ReadAllBytes(physicalPath);
        }

        private static string ResolvePath(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return null;

            if (Path.IsPathRooted(path))
                return path;

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                if (HttpContext.Current != null)
                    return HttpContext.Current.Server.MapPath(path);

                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Substring(2).Replace('/', Path.DirectorySeparatorChar));
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
