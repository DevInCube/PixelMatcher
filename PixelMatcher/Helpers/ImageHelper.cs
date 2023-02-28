using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixelMatcher.Helpers
{
    public static class ImageHelper
    {
        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppRgb);
            BitmapData data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppRgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapImage Convert(Image src)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                src.Save(ms, ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                if (image.CanFreeze)
                {
                    image.Freeze();
                }

                return image;
            }
        }

        public static void AdjustContrast(Image image, int adjustValue)
        {
            var contrast = (adjustValue + 100) / 100.0f;
            ApplyColorMatrix(image, CreateContrastMatrix(contrast));
        }

        private static void ApplyColorMatrix(Image image, ColorMatrix matrix)
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(matrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);

                graphics.DrawImage(image,
                    new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }
        }

        private static ColorMatrix CreateContrastMatrix(float contrast)
        {
            float t = (1.0f - contrast) / 2.0f;
            return new ColorMatrix(new float[][]{
                new float[] {contrast,  0,  0,  0,  0},
                new float[] {0,  contrast,  0,  0,  0},
                new float[] {0,  0,  contrast,  0,  0},
                new float[] {0,  0,  0,  1,  0},
                new float[] {t,  t,  t,  0,  1}
            });
        }
    }
}
