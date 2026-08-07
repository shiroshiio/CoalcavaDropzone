using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace COALCAVA_Dropzone
{
    public static class FontManager
    {
        // 1. Instantiating the PrivateFontCollection class
        private static readonly PrivateFontCollection FontCollection = new PrivateFontCollection();

        // Public Font properties to access anywhere in your app
        public static Font MontserratTitle { get; private set; }
        public static Font MontserratHeader { get; private set; }
        public static Font MontserratBody { get; private set; }
        public static Font MontserratButton { get; private set; }

        public static void Initialize()
        {
            try
            {
                // Path to your local TTF file in the output folder
                string boldPath = Path.Combine(Application.StartupPath, "Montserrat-Bold.ttf");
                string regularPath = Path.Combine(Application.StartupPath, "Montserrat-Regular.ttf");

                // Load files into the collection
                if (File.Exists(boldPath))
                    FontCollection.AddFontFile(boldPath);

                if (File.Exists(regularPath))
                    FontCollection.AddFontFile(regularPath);

                // Extract FontFamily
                FontFamily montserratFamily = FontCollection.Families[0];

                // Create pre-configured Font sizes
                MontserratTitle = new Font(montserratFamily, 16F, FontStyle.Bold);
                MontserratHeader = new Font(montserratFamily, 12F, FontStyle.Bold);
                MontserratButton = new Font(montserratFamily, 10F, FontStyle.Bold);
                MontserratBody = new Font(montserratFamily, 9.5F, FontStyle.Regular);
            }
            catch (Exception ex)
            {
                // Fallback to standard Segoe UI if font fails to load
                MontserratTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
                MontserratHeader = new Font("Segoe UI", 12F, FontStyle.Bold);
                MontserratButton = new Font("Segoe UI", 10F, FontStyle.Bold);
                MontserratBody = new Font("Segoe UI", 9.5F, FontStyle.Regular);

                System.Diagnostics.Debug.WriteLine("Font Load Error: " + ex.Message);
            }
        }
    }
}