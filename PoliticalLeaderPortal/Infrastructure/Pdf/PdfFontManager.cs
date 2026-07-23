namespace PoliticalLeaderPortal.Infrastructure.Pdf
{
    public static class PdfFontManager
    {
        public const string HindiFontFamily = "Arial";

        public static void Register()
        {
            // Windows and most IIS servers have Arial available. If you add a
            // Devanagari font later, register it here and update HindiFontFamily.
        }
    }
}
