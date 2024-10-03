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
using Microsoft.UI;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Drawing;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.System.Power;
using ABI.System.Numerics;
using System.Numerics;
using Microsoft.UI.Dispatching;
using System.Collections.Frozen;
using WinUISnippingTool.Views.UserControls;
using Windows.Graphics;


namespace WinUISnippingTool.ViewModels;

internal sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private readonly Dictionary<string, NotifyOnCompletionCollection<UIElement>> shapesDictionary;
    private readonly Dictionary<string, Microsoft.UI.Xaml.Controls.Image> imagesDictionary;
    private readonly Dictionary<MonitorLocation, SoftwareBitmap> softwareBitmaps;

    private readonly SnipPaintBase windowPaint;
    private readonly SnipPaintBase customShapePaint;
    private readonly SnipPaintBase rectangleSelectionPaint;
    private SnipPaintBase paintSnipKind;
    private readonly WindowPaint windowPaintSource;

    private string currentMonitorName;
    private MonitorLocation primaryMonitor;

    public event Action OnExitFromWindow;
    public Shape ResultFigure { get; private set; }
    public int ResultFigureActualWidth;
    public int ResultFigureActualHeight;

    public ImageSource CurrentShapeBmp { get; private set; }
    public RectInt32 WindowPosition { get; private set; }

    public string CurrentMonitorName => currentMonitorName;
    public bool IsShortcutResponce { get; private set; }
    public void SetCurrentMonitor(string monitorName) => currentMonitorName = monitorName;

    public override void SetWindowSize(Windows.Foundation.Size newSize)
    {
        base.SetWindowSize(newSize);
        windowPaintSource.SetWindowSize(newSize);
    }

    public void SetResponceType(bool isShortcut)
    {
        IsShortcutResponce = isShortcut;
    }

    public void SetPrimaryMonitor(MonitorLocation monitorLocation)
    {
        primaryMonitor = monitorLocation;
    }

    public SnipScreenWindowViewModel() : base()
    {
        LoadLocalization("uk-Ua");

        IsOverlayVisible = true;
        shapesDictionary = new();
        imagesDictionary = new();
        softwareBitmaps = new();
        windowPaintSource = new WindowPaint();
        windowPaint = windowPaintSource;
        customShapePaint = new CustomShapePaint();
        rectangleSelectionPaint = new RectangleSelectionPaint();
    }

    public void ResetModel()
    {
        IsOverlayVisible = true;
        CurrentShapeBmp = null;
    }

    public void AddSoftwareBitmapForCurrentMonitor(MonitorLocation location, SoftwareBitmap bitmap)
    {
        softwareBitmaps.Add(location, bitmap);
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
        var source = (SoftwareBitmapSource)imagesDictionary[currentMonitorName].Source;
        
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

    public void SetSelectedItem(SnipKinds kind)
    {
        SelectedSnipKind = SnipShapeKinds.First(shapeKind => shapeKind.Kind == kind);
    }

    protected override void SelectionChangedCallback()
    {
        DefineKind(SelectedSnipKind.Kind);
        base.SelectionChangedCallback();
    }

    private void DefineKind(SnipKinds kind)
    {
        switch (kind)
        {
            case SnipKinds.Recntangular: paintSnipKind = rectangleSelectionPaint; break;
            case SnipKinds.Window: paintSnipKind = windowPaint; break;
            case SnipKinds.CustomShape: paintSnipKind = customShapePaint; break;
            case SnipKinds.AllWindows: paintSnipKind = null; break;
        }
    }

    public void OnPointerPressed(Windows.Foundation.Point position)
    {
        paintSnipKind?.OnPointerPressed(position);
        IsOverlayVisible = false;
    }

    public void OnPointerMoved(Windows.Foundation.Point position)
    {
        paintSnipKind?.OnPointerMoved(position);
    }

    private Task CreateSaveBmpToAllPlacesTask(SoftwareBitmap softwareBitmap)
    {
        var saveToFolderTask = PicturesFolderExtensions.SaveAsync(softwareBitmap)
                    .ContinueWith(t =>
                    {
                        if (IsShortcutResponce)
                        {
                            var imageUri = new Uri("file:///" + t.Result.Path);
                            var builder = new AppNotificationBuilder()
                            .SetInlineImage(imageUri)
                            .AddArgument("snapshotStatus", "snapshotTaken")
                            .AddArgument("snapshotUri", imageUri.ToString())
                            .AddArgument("snapshotWidth", ResultFigureActualWidth.ToString())
                            .AddArgument("snapshotHeight", ResultFigureActualHeight.ToString());

                            var notification = builder.BuildNotification();
                            var notificationManager = AppNotificationManager.Default;
                            notificationManager.Show(notification);
                        }
                    });

        var clipboardTask =  ClipboardExtensions.CopyAsync(softwareBitmap);

        return Task.WhenAll(saveToFolderTask, clipboardTask);
    }

    private async Task<SoftwareBitmap> GetSingleMonitorSnapshot()
    {
        var softwareBitmap = await RenderExtensions.ProcessShapeAsync(ResultFigure);

        paintSnipKind.Clear();
        ResultFigureActualWidth = softwareBitmap.PixelWidth;
        ResultFigureActualHeight = softwareBitmap.PixelHeight;

        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(softwareBitmap);

        CurrentShapeBmp = source;

        return softwareBitmap;
    }

    private async Task<SoftwareBitmap> GetAllMonitorsSnapshot()
    {
        var images = softwareBitmaps.Values.AsEnumerable();
        var softwareBitmap = await RenderExtensions.ProcessImagesAsync(primaryMonitor, images);
        
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(softwareBitmap);
        
        CurrentShapeBmp = source;

        ResultFigureActualWidth = softwareBitmap.PixelWidth;
        ResultFigureActualHeight = softwareBitmap.PixelHeight;
        return softwareBitmap;
    }

    public async Task OnPointerReleased(Windows.Foundation.Point position)
    {
        if (paintSnipKind is not null)
        {
            ResultFigure = paintSnipKind.OnPointerReleased(position);

            if (ResultFigure is not null
                && SelectedSnipKind.Kind != SnipKinds.AllWindows
                && SnipControl.CaptureKind == CaptureType.Photo)
            {
                var sbitmap = await GetSingleMonitorSnapshot();
                await CreateSaveBmpToAllPlacesTask(sbitmap);
            }
            else if (SnipControl.CaptureKind == CaptureType.Video)
            {
                var posX = (int)ResultFigure.ActualOffset.X;
                var posY = (int)ResultFigure.ActualOffset.Y;
                var absXDiff = (int)Math.Abs(position.X - posX);
                var absYDiff = (int)Math.Abs(position.Y - posY);

                WindowPosition = new(posX, posY, absXDiff, absYDiff);
            }
        }
        else if (SelectedSnipKind.Kind == SnipKinds.AllWindows)
        {
            var sbitmap = await GetAllMonitorsSnapshot();
            Task.Run(() => Console.Beep(500, 200));
            await CreateSaveBmpToAllPlacesTask(sbitmap);
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

    public void Exit()
    {
        foreach(var item in shapesDictionary)
        {
            item.Value.Clear();
        }

        foreach(var item in imagesDictionary)
        {
            var softwareBitmapSource = (SoftwareBitmapSource)item.Value.Source;
            softwareBitmapSource.Dispose();
            item.Value.Source = null;
        }

        foreach(var softwareBitmap in softwareBitmaps.Values)
        {
            softwareBitmap.Dispose();
        }

        softwareBitmaps.Clear();

        if (IsShortcutResponce)
        {
            CurrentShapeBmp = null;
        }

        OnExitFromWindow?.Invoke();
    }
}
