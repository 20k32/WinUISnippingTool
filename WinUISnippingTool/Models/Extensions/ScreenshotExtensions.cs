using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using Windows.Graphics.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.ExceptionServices;
using System.Diagnostics;


namespace WinUISnippingTool.Models.Extensions;

internal static class ScreenshotExtensions
{
    public static BitmapImage GetBitmapImageScreenshotForArea
        (Point upperLeftSource, Point upperLeftDestination, Windows.Foundation.Size size)
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

                    using (var randomStream = stream.AsRandomAccessStream())
                    {
                        bitmapImage.SetSource(randomStream);
                    }
                }
            }
        }

        return bitmapImage;
    }

    public static async Task<SoftwareBitmap> GetSoftwareBitmapImageScreenshotForAreaAsync
        (Point upperLeftSource, Point upperLeftDestination, Windows.Foundation.Size size)
    {
        SoftwareBitmap softwareBitmap = null;
        using (var bmpScreenshot = new Bitmap((int)size.Width, (int)size.Height))
        {
            using (var g = Graphics.FromImage(bmpScreenshot))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.CopyFromScreen(upperLeftSource, upperLeftDestination, bmpScreenshot.Size);
            }

            using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                using(var averageStream = stream.AsStream())
                {
                    bmpScreenshot.Save(averageStream, ImageFormat.Jpeg);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    stream.Seek(0);
                    // access violation here: 
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
            }
        }

        return softwareBitmap;
    }
}
