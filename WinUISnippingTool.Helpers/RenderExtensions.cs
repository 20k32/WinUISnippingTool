using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
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
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WinUISnippingTool.Helpers.Saving;

namespace WinUISnippingTool.Helpers;

public static class RenderExtensions
{
    private static double minX;
    private static double minY;
    private static double maxX;
    private static double maxY;

    private static void ResetBounds()
    {
        minX = double.MaxValue;
        minY = double.MaxValue;
        maxX = double.MinValue;
        maxY = double.MinValue;
    }

    private static void CalculateNewBounds(Point point)
    {
        minX = Math.Min(minX, point.X);
        minY = Math.Min(minY, point.Y);
        maxX = Math.Max(maxX, point.X);
        maxY = Math.Max(maxY, point.Y);

        /*if (point.X < minX) minX = point.X;
        if (point.Y < minY) minY = point.Y;
        if (point.X > maxX) maxX = point.X;
        if (point.Y > maxY) maxY = point.Y;*/
    }

    private static Rect GetBounds() 
        => new Rect(minX, minY, maxX - minX, maxY - minY);

    public static async Task<SoftwareBitmap> ProcessShapeAsync(Shape element)
    {
        var renderTargetBitmap = new RenderTargetBitmap();
        await renderTargetBitmap.RenderAsync(element);
        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

        return SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
    }

    public static async Task<SoftwareBitmap> ProcessPointsAsync(PointCollection points, SoftwareBitmap bitmap)
    {
        ResetBounds();

        var vectorPoints = new List<Vector2>(points.Count);

        foreach (var point in points)
        {
            vectorPoints.Add(new((float)point.X, (float)point.Y));
            CalculateNewBounds(point);
        }

        var bounds = GetBounds();
        var device = CanvasDevice.GetSharedDevice();

        var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(device, bitmap);
        var renderTarget = new CanvasRenderTarget(device, (float)bounds.Width, (float)bounds.Height, BitmapSavingConstants.Dpi);

        using (var drawingSession = renderTarget.CreateDrawingSession())
        {
            drawingSession.Clear(Colors.Transparent);

            var vectorPointsArray = vectorPoints.ToArray();
            var canvasGeometry = CanvasGeometry.CreatePolygon(device, vectorPointsArray);

            var matrix = Matrix3x2.CreateTranslation((float)-bounds.X, (float)-bounds.Y);

            using (var layer = drawingSession.CreateLayer(1.0f, canvasGeometry, matrix))
            {
                drawingSession.DrawImage(canvasBitmap, 0, 0, bounds);
            }
        }

        SoftwareBitmap result = null;
        using (var stream = new InMemoryRandomAccessStream())
        {
            await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            result = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        return result;
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
                prevBoundWidth += image.Bounds.Width;
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
