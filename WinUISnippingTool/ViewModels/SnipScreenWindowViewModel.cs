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
using Microsoft.UI.Xaml.Shapes;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using WinUISnippingTool.Views;
using WinUISnippingTool.Models.Extensions;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;



namespace WinUISnippingTool.ViewModels;

internal sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private PaintBase windowPaint;
    private PaintBase customShapeKind;
    private PaintBase rectangleSelectionPaint;
    private PaintBase paintSnipKind;
    private bool shortcutResponce;
    public Shape ResultFigure { get; private set; }
    public int ResultFigureActualWidth;
    public int ResultFigureActualHeight;

    public void SetResponceType(bool isShortcut)
    {
        shortcutResponce = isShortcut;
    }

    public SnipScreenWindowViewModel()
    { }


    public SnipScreenWindowViewModel(BitmapImage bmpImage, SnipKinds kind) : base()
    {
        currentImage = new Image();
        currentImage.Opacity = 0.3;
        currentImage.Source = bmpImage;

        CanvasItems.Add(currentImage);
        CanvasWidth = bmpImage.PixelWidth;
        CanvasHeight = bmpImage.PixelHeight;
        windowPaint = new WindowPaint(CanvasItems, new Size(2560, 1440), currentImage.Source);
        customShapeKind = new CustomShapePaint(CanvasItems, currentImage.Source);
        rectangleSelectionPaint = new RectangleSelectionPaint(CanvasItems, currentImage.Source);

        DefineKind(kind);
    }

    public void SetBitmapImage(BitmapImage bmpImage)
    {
        currentImage = new Image();
        currentImage.Opacity = 0.3;
        currentImage.Source = bmpImage;

        CanvasItems.Add(currentImage);
        CanvasWidth = bmpImage.PixelWidth;
        CanvasHeight = bmpImage.PixelHeight;
        windowPaint = new WindowPaint(CanvasItems, new Size(2560, 1440), currentImage.Source);
        customShapeKind = new CustomShapePaint(CanvasItems, currentImage.Source);
        rectangleSelectionPaint = new RectangleSelectionPaint(CanvasItems, currentImage.Source);
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

    private bool pointerReleased;

    public async Task OnPointerReleased(Point position)
    {
        ResultFigure = paintSnipKind.OnPointerReleased(position);
        var renderBitmap = new RenderTargetBitmap();
        await renderBitmap.RenderAsync(ResultFigure);
        var pixelBuffer = await renderBitmap.GetPixelsAsync();

        var storageFile = await PicturesLibraryExtensions.SaveAsync(renderBitmap, pixelBuffer);
        await ClipboardExtensions.CopyAsync(renderBitmap, pixelBuffer);

        ResultFigureActualWidth = renderBitmap.PixelWidth;
        ResultFigureActualHeight = renderBitmap.PixelHeight;

        if (shortcutResponce)
        {
            var imageUri = new Uri("file:///" + storageFile.Path); //todo: better use temp folder
            var builder = new AppNotificationBuilder()
                .SetInlineImage(imageUri)
                .AddArgument("snapshotStatus", "snapshotTaken")
                .AddArgument("snapshotUri", imageUri.ToString())
                .AddArgument("snapshotWidth", ResultFigureActualWidth.ToString())
                .AddArgument("snapshotHeight", ResultFigureActualHeight.ToString());

            var notificationManager = AppNotificationManager.Default;
            notificationManager.Show(builder.BuildNotification());
        }
    }

    public void Exit()
    { }
}
