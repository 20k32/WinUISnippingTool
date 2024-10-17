using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using Windows.Graphics.Imaging;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Graphics.Capture;
using Windows.Win32.Graphics.Gdi;
using Windows.Graphics.DirectX;
using WinUISnippingTool.Helpers.DirectX;


namespace WinUISnippingTool.Helpers;

public static class ScreenshotExtensions
{

    /// <summary>
    /// slow, uses a bit memory
    /// </summary>
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

    /// <summary>
    /// slow, uses a bit memory
    /// </summary>
    /// 
    public static async Task<SoftwareBitmap> GetSoftwareBitmapImageScreenshotForAreaAsync
        (Point upperLeftSource, Point upperLeftDestination, Windows.Foundation.Size size)
    {
        SoftwareBitmap softwareBitmap = null;
        using (var bmpScreenshot = new Bitmap((int)size.Width, (int)size.Height))
        {
            Debug.WriteLine($"Bmp: [{bmpScreenshot.Size.Width}, {bmpScreenshot.Size.Height}]" +
                $"\nCurr: [{size.Width}, {size.Height}]");

            using (var g = Graphics.FromImage(bmpScreenshot))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CopyFromScreen(upperLeftSource, upperLeftDestination, bmpScreenshot.Size);
            }

            using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                using (var averageStream = stream.AsStreamForWrite(8000))
                {
                    bmpScreenshot.Save(averageStream, ImageFormat.Png);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                    Debug.WriteLine("Try get software bitmap");

                    softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                    Debug.WriteLine($"Screenshot taken {DateTime.Now.ToLongTimeString()}");
                }
            }
        }

        return softwareBitmap;
    }

    /// <summary>
    /// fast, uses much memory
    /// </summary>
    public static async Task<SoftwareBitmap> GetSoftwareBitmapImageScreenshotForAreaAsync
        (Point upperLeftSource, nint handleMonitor)
    {
        SoftwareBitmap softwareBitmap = null;
        var graphicsItem = GraphicsCaptureItemExtensions.CreateItemForMonitor((HMONITOR)handleMonitor);
        var device3d = Direct3D11Helpers.CreateDevice();
        
        var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                device3d,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                1,
                graphicsItem.Size);
        var session = framePool.CreateCaptureSession(graphicsItem);

        var taskCompletion = new TaskCompletionSource<Direct3D11CaptureFrame>();
        framePool.FrameArrived += (s, a) =>
        {
            var frame = s.TryGetNextFrame();
            taskCompletion.SetResult(frame);
        };
        session.StartCapture();

        var frame = await taskCompletion.Task;
        framePool.Dispose();
        session.Dispose();

        var surface = frame.Surface;

        
        softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(surface, BitmapAlphaMode.Premultiplied); // access violation here

        return softwareBitmap;
    }
}
