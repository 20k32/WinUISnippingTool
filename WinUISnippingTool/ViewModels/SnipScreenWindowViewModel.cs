using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Paint;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Shapes;
using WinUISnippingTool.Models.Extensions;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using WinUISnippingTool.Models.Items;
using System.Linq;


namespace WinUISnippingTool.ViewModels;

internal sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private PaintBase windowPaint;
    private PaintBase customShapeKind;
    private PaintBase rectangleSelectionPaint;
    private PaintBase paintSnipKind;
    private bool shortcutResponce;
    public bool ExitRequested;
    public Shape ResultFigure { get; private set; }
    public int ResultFigureActualWidth;
    public int ResultFigureActualHeight;
    public RenderTargetBitmap CurrentShapeBmp;

    public void SetResponceType(bool isShortcut)
    {
        shortcutResponce = isShortcut;
    }

    public SnipScreenWindowViewModel() : base()
    {
        TrySetAndLoadLocalization("uk-UA");
        IsOverlayVisible = true;
    }

    public void SetSelectedItem(SnipKinds kind) // bug: can't pass reference to SelectedSnipKind from main VM
    {
        DefineKind(kind);
        SelectedSnipKind = SnipShapeKinds.First(shapeKind => shapeKind.Kind == kind);
    }

    protected override void SelectionChangedCallback()
    {
        base.SelectionChangedCallback();
        DefineKind(SelectedSnipKind.Kind);
    }

    private void DefineKind(SnipKinds kind)
    {
        switch (kind)
        {
            case SnipKinds.Recntangular: paintSnipKind = rectangleSelectionPaint; break;
            case SnipKinds.Window: paintSnipKind = windowPaint; break;
            case SnipKinds.CustomShape: paintSnipKind = customShapeKind; break;
            case SnipKinds.AllWindows: throw new NotImplementedException();
        }
    }

    public void SetBitmapImage(BitmapImage bmpImage)
    {
        currentImage = new Image();
        currentImage.Opacity = 0.3;
        currentImage.Source = bmpImage;

        CanvasItems.Add(currentImage);
        CanvasWidth = bmpImage.PixelWidth;
        CanvasHeight = bmpImage.PixelHeight;

        windowPaint = new WindowPaint(CanvasItems, defaultWindowSize, currentImage.Source);
        customShapeKind = new CustomShapePaint(CanvasItems, currentImage.Source);
        rectangleSelectionPaint = new RectangleSelectionPaint(CanvasItems, currentImage.Source);
    }


    public void OnPointerPressed(Point position)
    {
        paintSnipKind.OnPointerPressed(position);
        IsOverlayVisible = false;
    }

    public void OnPointerMoved(Point position)
    {
        paintSnipKind.OnPointerMoved(position);
    }

    public async Task OnPointerReleased(Point position)
    {
        if(paintSnipKind is null)
        {
            throw new ArgumentNullException(nameof(paintSnipKind));
        }

        ResultFigure = paintSnipKind.OnPointerReleased(position);
        CurrentShapeBmp = new RenderTargetBitmap();
        
        await CurrentShapeBmp.RenderAsync(ResultFigure);
        paintSnipKind.Clear();
        var pixelBuffer = await CurrentShapeBmp.GetPixelsAsync();
        ResultFigureActualWidth = CurrentShapeBmp.PixelWidth;
        ResultFigureActualHeight = CurrentShapeBmp.PixelHeight;
        var pixels = pixelBuffer.ToArray();

        var task1 = PicturesFolderExtensions.SaveAsync((uint)CurrentShapeBmp.PixelWidth, (uint)CurrentShapeBmp.PixelHeight, pixels).ContinueWith(t =>
        {
            if (shortcutResponce)
            {
                var imageUri = new Uri("file:///" + t.Result.Path);
                var builder = new AppNotificationBuilder()
                    .SetInlineImage(imageUri)
                    .AddArgument("snapshotStatus", "snapshotTaken")
                    .AddArgument("snapshotUri", imageUri.ToString())
                    .AddArgument("snapshotWidth", ResultFigureActualWidth.ToString())
                    .AddArgument("snapshotHeight", ResultFigureActualHeight.ToString());


                var notificationManager = AppNotificationManager.Default;
                notificationManager.Show(builder.BuildNotification());
            }
        });

        var task2 = ClipboardExtensions.CopyAsync((uint)CurrentShapeBmp.PixelWidth, (uint)CurrentShapeBmp.PixelHeight, pixels);

        await Task.WhenAll(task1, task2);
    }

    private bool isOverlayVisible;

    public bool IsOverlayVisible
    {
        get => isOverlayVisible;
        set
        {
            if (isOverlayVisible != value)
            {
                isOverlayVisible = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public void Exit()
    {
        ExitRequested = true;
    }
}
