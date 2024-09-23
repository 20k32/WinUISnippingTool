using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using Windows.Foundation;


namespace WinUISnippingTool.Models;

internal static class ScreenshotHelper
{
    public static BitmapImage GetBitmapImageScreenshotForArea(int width, int height)
    {
        var bitmapImage = new BitmapImage();

        using (var bmpScreenshot = new Bitmap(width, height))
        {
            using (var g = Graphics.FromImage(bmpScreenshot))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.CopyFromScreen(0, 0, 0, 0, bmpScreenshot.Size);

                using (var stream = new MemoryStream())
                {
                    bmpScreenshot.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;
                    bitmapImage.SetSource(stream.AsRandomAccessStream());
                }
            }
        }

        return bitmapImage;
    }
}
