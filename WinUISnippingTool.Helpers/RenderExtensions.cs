using CommunityToolkit.WinUI.UI;
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
using System.Net.WebSockets;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WinUISnippingTool.Helpers.Saving;

namespace WinUISnippingTool.Helpers;

public static class RenderExtensions
{ 
    public static async Task<SoftwareBitmap> ProcessShapeAsync(Shape element)
    {
        var renderTargetBitmap = new RenderTargetBitmap();
        await renderTargetBitmap.RenderAsync(element);
        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

        return SoftwareBitmap.CreateCopyFromBuffer(pixelBuffer, BitmapPixelFormat.Bgra8, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
    }

    public static async Task<SoftwareBitmap> ProcessPointsAsync(PointCollection points, SoftwareBitmap bitmap)
    {
        SoftwareBitmap result = null;

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        var vectorPointsArray = new Vector2[points.Count];
        var index = default(int);

        foreach (var point in points)
        {
            var vectorPoint = new Vector2((float)point.X, (float)point.Y);
            vectorPointsArray[index++] = vectorPoint;

            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        var bounds = new Rect(minX, minY, maxX - minX, maxY - minY);

        using (var device = CanvasDevice.GetSharedDevice())
        using (var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(device, bitmap))
        using (var renderTarget = new CanvasRenderTarget(device, (float)bounds.Width, (float)bounds.Height, 96))
        {
            using (var drawingSession = renderTarget.CreateDrawingSession())
            {
                drawingSession.Clear(Colors.Transparent);

                var matrix = Matrix3x2.CreateTranslation((float)-bounds.X, (float)-bounds.Y);

                using (var canvasGeometry = CanvasGeometry.CreatePolygon(device, vectorPointsArray))
                using (var layer = drawingSession.CreateLayer(1.0f, canvasGeometry, matrix))
                {
                    drawingSession.DrawImage(canvasBitmap, 0, 0, bounds);
                }
            }

            using (var stream = new InMemoryRandomAccessStream())
            {
                await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                result = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
        }

        return result;
    }

    public static async Task<SoftwareBitmap> ProcessImagesAsync(MonitorLocation primaryMonitor, IEnumerable<SoftwareBitmap> images)
    {
        SoftwareBitmap result = null;
        var canvasBitmaps = new List<CanvasBitmap>();
        var desiredSize = WindowExtensions.CalculateDesiredSizeForMonitor(primaryMonitor, out var dpiTuple);
        
        var height = -1;
        var width = default(int);
        var prevBoundWidth = default(double);

        using (var device = CanvasDevice.GetSharedDevice())
        {
            foreach (var image in images)
            {
                width += image.PixelWidth;

                if (height < image.PixelHeight)
                {
                    height = image.PixelHeight;
                }

                var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(device, image);
                canvasBitmaps.Add(canvasBitmap);
                prevBoundWidth += canvasBitmap.Bounds.Width;
            }

            var scaleX = desiredSize.Width / width;
            var scaleY = desiredSize.Height / height;
            var minScale = Math.Min(scaleX, scaleY);
            var matrix3x2 = Matrix3x2.CreateScale((float)minScale);
            var croppedHeight = height * minScale;
            var croppedWidth = prevBoundWidth * minScale;


            using (var renderTarget = new CanvasRenderTarget(device, (int)croppedWidth, (int)croppedHeight, dpiTuple.dpiX))
            {
                using (var drawingSession = renderTarget.CreateDrawingSession())
                {
                    drawingSession.Clear(Colors.Transparent);

                    drawingSession.Transform = matrix3x2;
                    canvasBitmaps.Reverse();

                    prevBoundWidth = default(double);
                    foreach (var bitmap in canvasBitmaps)
                    {
                        using (bitmap)
                        {
                            drawingSession.DrawImage(bitmap, (float)prevBoundWidth, 0);
                            prevBoundWidth += bitmap.Bounds.Width;
                        }
                    }
                }

                using (var stream = new InMemoryRandomAccessStream())
                {
                    await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    result = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
            }
        }
        
        return result;
    }

    public static async Task<SoftwareBitmap> ChangeResolutionAsync(SoftwareBitmap originalBitmap, int targetWidth, int targetHeight)
    {
        using (var stream = new InMemoryRandomAccessStream())
        {
            var encoder = await BitmapEncoder.CreateAsync(BitmapSavingConstants.EncoderId, stream);
            encoder.SetSoftwareBitmap(originalBitmap);
            encoder.BitmapTransform.ScaledWidth = (uint)targetWidth;
            encoder.BitmapTransform.ScaledHeight = (uint)targetHeight;
            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

            await encoder.FlushAsync();

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var resizedBitmap = await decoder.GetSoftwareBitmapAsync(originalBitmap.BitmapPixelFormat, originalBitmap.BitmapAlphaMode);

            return resizedBitmap;
        }
    }
}
