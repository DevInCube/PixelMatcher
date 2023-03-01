using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace PixelMatcher.Helpers
{
    internal static class ScreenHelper
    {
        public static BitmapImage GetScreenshot()
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            using (var bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                }
                return ImageHelper.Convert(bitmap);
            }
        }
    }
}
