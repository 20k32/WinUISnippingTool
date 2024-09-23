using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Paint;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;



namespace WinUISnippingTool.ViewModels;

internal sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private readonly PaintBase windowPaint;
    private readonly PaintBase customShapeKind;
    private readonly PaintBase rectangleSelectionPaint;
    private PaintBase paintSnipKind;

    public SnipScreenWindowViewModel(BitmapImage bmpImage, SnipKinds kind) : base()
    {
        currentImage = new Image();
        currentImage.Opacity = 0.8;
        currentImage.Source = bmpImage;
        CanvasItems.Add(currentImage);
        CanvasWidth = bmpImage.PixelWidth;
        CanvasHeight = bmpImage.PixelHeight;
        windowPaint = new WindowPaint(CanvasItems, new Size(2560, 1440));
        customShapeKind = new CustomShapePaint(CanvasItems);
        rectangleSelectionPaint = new RectangleSelectionPaint(CanvasItems);
        DefineKind(kind);
    }

    public void DefineKind(SnipKinds kind)
    {
        switch (kind)
        {
            case SnipKinds.Recntangular: paintSnipKind = rectangleSelectionPaint; break;
            case SnipKinds.Window: paintSnipKind = windowPaint; break;
            case SnipKinds.CustomShape: paintSnipKind = customShapeKind; break;
            case SnipKinds.AllWindows: throw new NotImplementedException();
        }
    }

    public void OnPointerPressed(Point position)
    {
        paintSnipKind.OnPointerPressed(position);
    }

    public void OnPointerMoved(Point position)
    {
        paintSnipKind.OnPointerMoved(position);
    }

    public async Task OnPointerReleased(Point position)
    {
        try
        {
            currentImage.Clip = new()
            {
                Rect = new(0, 0, 50, 50)
            };

            //var figure = paintSnipKind.OnPointerReleased(position);


            var renderTargetBitmap = new RenderTargetBitmap();
            renderTargetBitmap.RenderAsync(currentImage).GetResults();

            var pixelBuffer = renderTargetBitmap.GetPixelsAsync().Get();
            var width = renderTargetBitmap.PixelWidth;
            var height = renderTargetBitmap.PixelHeight;

            var croppedBitmap = new WriteableBitmap(width, height);
            pixelBuffer.CopyTo(croppedBitmap.PixelBuffer);

            using var stream = pixelBuffer.AsStream().AsRandomAccessStream();

            //var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height);
            //softwareBitmap.CopyFromBuffer(croppedBitmap.PixelBuffer);

            DataPackage dataPackage = new();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            var streamR = RandomAccessStreamReference.CreateFromStream(stream);
            dataPackage.SetBitmap(streamR);
            Clipboard.SetContent(dataPackage);

            var package = Clipboard.GetContent();
        }
        catch (Exception ex)
        {
            var e = ex;
            var asla = currentImage.IsLoaded;
        }

    }

    public void Exit()
    { }
}
