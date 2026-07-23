using QuestPDF.Infrastructure;

namespace PoliticalLeaderPortal.Infrastructure.Pdf
{
    public static class PdfConfiguration
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
                return;

            QuestPDF.Settings.License = LicenseType.Community;
            _initialized = true;
        }
    }
}
