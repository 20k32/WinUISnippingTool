using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;


namespace WinUISnippingTool.Models;

internal static class ScreenshotHelper
{
    public static BitmapImage GetBitmapImageScreenshotForArea(Point upperLeftSource, Point upperLeftDestination, Windows.Foundation.Size size)
    {
        var bitmapImage = new BitmapImage();

        using (var bmpScreenshot = new Bitmap((int)size.Width, (int)size.Height))
        {
            using (var g = Graphics.FromImage(bmpScreenshot))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.CopyFromScreen(upperLeftSource, upperLeftDestination, bmpScreenshot.Size);

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
