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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;


namespace WinUISnippingTool.ViewModels;

internal sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private string currentMonitorName;
    private readonly Dictionary<string, NotifyOnCompletionCollection<UIElement>> shapesDictionary;
    private readonly Dictionary<string, Image> imagesDictionary;
    private readonly SnipPaintBase windowPaint;
    private readonly SnipPaintBase customShapePaint;
    private readonly SnipPaintBase rectangleSelectionPaint;
    private readonly WindowPaint windowPaintSource;
    private SnipPaintBase paintSnipKind;
    private bool shortcutResponce;
    public event Action OnExitFromWindow;

    public bool ExitRequested;
    public Shape ResultFigure { get; private set; }
    public int ResultFigureActualWidth;
    public int ResultFigureActualHeight;
    public RenderTargetBitmap CurrentShapeBmp;

    public string CurrentMonitorName => currentMonitorName;
    public void SetCurrentMonitor(string monitorName) => currentMonitorName = monitorName;
    
    public override void SetWindowSize(Size newSize)
    {
        base.SetWindowSize(newSize);
        windowPaintSource.SetWindowSize(newSize);
    }

    public void SetResponceType(bool isShortcut)
    {
        shortcutResponce = isShortcut;
    }

    public SnipScreenWindowViewModel() : base()
    {
        TrySetAndLoadLocalization("uk-UA");

        IsOverlayVisible = true;
        shapesDictionary = new();
        imagesDictionary = new();
        windowPaintSource = new WindowPaint();
        windowPaint = windowPaintSource;
        customShapePaint = new CustomShapePaint();
        rectangleSelectionPaint = new RectangleSelectionPaint();
    }

    public void ResetModel()
    {
        IsOverlayVisible = true;
    }

    public void SetShapeSourceForCurrentMonitor()
    {
        var canvasItem = shapesDictionary[currentMonitorName];

        windowPaintSource.SetShapeSource(canvasItem);
        customShapePaint.SetShapeSource(canvasItem);
        rectangleSelectionPaint.SetShapeSource(canvasItem);
    }

    public void AddImageSourceAndBrushFillForCurentMonitor(ImageSource source)
    {
        var canvasItem = shapesDictionary[currentMonitorName];

        var image = imagesDictionary[currentMonitorName];
        image.Source = source;
        image.Opacity = 0.3;
        canvasItem.Add(image);

        rectangleSelectionPaint.SetImageFill(source);
        windowPaint.SetImageFill(source);
        customShapePaint.SetImageFill(source);
    }

    public void AddShapeSourceForCurrentMonitor()
    {
        var canvasItem = shapesDictionary[currentMonitorName];

        rectangleSelectionPaint.SetShapeSource(canvasItem);
        windowPaint.SetShapeSource(canvasItem);
        customShapePaint.SetShapeSource(canvasItem);
    }

    public void SetImageSourceForCurrentMonitor()
    {        
        var source = imagesDictionary[currentMonitorName].Source;

        rectangleSelectionPaint.SetImageFill(source);
        windowPaint.SetImageFill(source);
        customShapePaint.SetImageFill(source);
    }

    public NotifyOnCompletionCollection<UIElement> GetOrAddCollectionForCurrentMonitor()
    {
        NotifyOnCompletionCollection<UIElement> result;

        if (shapesDictionary.TryGetValue(currentMonitorName, out NotifyOnCompletionCollection<UIElement> value))
        {
            result = value;
        }
        else
        {
            result = new();
            shapesDictionary.Add(currentMonitorName, result);
            imagesDictionary.Add(currentMonitorName, new());
        }

        return result;
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
            case SnipKinds.CustomShape: paintSnipKind = customShapePaint; break;
            case SnipKinds.AllWindows: throw new NotImplementedException();
        }
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
        ArgumentNullException.ThrowIfNull(paintSnipKind);

        ResultFigure = paintSnipKind.OnPointerReleased(position);
        if(ResultFigure is not null)
        {
            CurrentShapeBmp = new RenderTargetBitmap();

            await CurrentShapeBmp.RenderAsync(ResultFigure);
            paintSnipKind.Clear();
            var pixelBuffer = await CurrentShapeBmp.GetPixelsAsync();
            ResultFigureActualWidth = CurrentShapeBmp.PixelWidth;
            ResultFigureActualHeight = CurrentShapeBmp.PixelHeight;
            var pixels = pixelBuffer.ToArray();

            var saveToFileTask = PicturesFolderExtensions.SaveAsync((uint)CurrentShapeBmp.PixelWidth, (uint)CurrentShapeBmp.PixelHeight, pixels)
                .ContinueWith(t =>
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

            var saveToClipboardTask = ClipboardExtensions.CopyAsync((uint)CurrentShapeBmp.PixelWidth, (uint)CurrentShapeBmp.PixelHeight, pixels);

            await Task.WhenAll(saveToFileTask, saveToClipboardTask);
        }
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

    public void Exit(bool exitRequested)
    {
        ExitRequested = exitRequested;

        foreach(var item in shapesDictionary)
        {
            item.Value.Clear();
        }

        foreach(var item in imagesDictionary)
        {
            item.Value.Source = null;
        }

        OnExitFromWindow?.Invoke();
    }
}
