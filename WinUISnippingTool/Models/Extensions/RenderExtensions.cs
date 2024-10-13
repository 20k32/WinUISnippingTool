using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WinUISnippingTool.Models.MonitorInfo;

namespace WinUISnippingTool.Models.Extensions;

internal static class RenderExtensions
{
    public static async Task<SoftwareBitmap> ProcessShapeAsync(Shape element)
    {
        var renderTargetBitmap = new RenderTargetBitmap();
        await renderTargetBitmap.RenderAsync(element);
        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

        return SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
    }

    public static async Task<SoftwareBitmap> ProcessImagesAsync(MonitorLocation primaryMonitor, IEnumerable<SoftwareBitmap> images)
    {
        SoftwareBitmap result = null;

        int height = images.First().PixelHeight;
        int width = images.First().PixelWidth;

        foreach (var item in images.Skip(1))
        {
            width += item.PixelWidth;

            if (height < item.PixelHeight)
            {
                height = item.PixelHeight;
            }
        }

        var scaleX = primaryMonitor.Location.Width / (double)width;
        var scaleY = primaryMonitor.Location.Height / (double)height;
        var minScale = Math.Min(scaleX, scaleY);

        var newHeight = height * minScale;
        var newWidth = width * minScale;

        var device = CanvasDevice.GetSharedDevice();
        var renderTarget = new CanvasRenderTarget(device, (int)newWidth, (int)newHeight, BitmapSavingConstants.Dpi);

        using (var drawingSession = renderTarget.CreateDrawingSession())
        {
            drawingSession.Clear(Colors.Transparent);

            var firstBmp = images.Last();

            var firstImage = CanvasBitmap.CreateFromSoftwareBitmap(device, firstBmp);
            var newBoundWidth = (float)(firstImage.Bounds.Width * minScale);
            var newBoundHeight = (float)(firstImage.Bounds.Height * minScale);

            var matrix = System.Numerics.Matrix3x2.CreateScale((float)minScale, (float)minScale);
            drawingSession.Transform = matrix;

            drawingSession.DrawImage(firstImage, 0, 0);

            var prevBoundWidth = firstImage.Bounds.Width;

            foreach (var bitmap in images.Reverse().Skip(1))
            {
                var image = CanvasBitmap.CreateFromSoftwareBitmap(device, bitmap);

                newBoundWidth = (float)(image.Bounds.Width * minScale);
                newBoundHeight = (float)(image.Bounds.Height * minScale);

                drawingSession.DrawImage(image, (float)prevBoundWidth, 0);
                prevBoundWidth += newBoundWidth;
            }
        }

        using (var stream = new InMemoryRandomAccessStream())
        {
            await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            result = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        return result;
    }
}
