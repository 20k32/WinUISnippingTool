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
using Windows.Graphics.Capture;
using WinUISnippingTool.Models.VideoCapture;
using Windows.Win32.Graphics.Gdi;
using Windows.Graphics.DirectX;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;


namespace WinUISnippingTool.Models.Extensions;

internal static class ScreenshotExtensions
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

                    softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
            }
        }

        return softwareBitmap;
    }

    /// <summary>
    /// fast, uses much memory
    /// </summary>
    public static async Task<SoftwareBitmap> GetSoftwareBitmapImageScreenshotForAreaAsync
        (Point upperLeftSource, Point upperLeftDestination, nint handleMonitor)
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
        // todo if app crashes: access violation here.
        softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(surface, BitmapAlphaMode.Premultiplied);

        return softwareBitmap;
    }
}
