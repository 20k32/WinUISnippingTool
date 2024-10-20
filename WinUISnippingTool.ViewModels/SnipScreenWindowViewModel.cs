using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Paint;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using WinUISnippingTool.Models.Items;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics.Imaging;
using Windows.Graphics;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI;
using WinUISnippingTool.Models.MonitorInfo;
using WinUISnippingTool.Helpers.Saving;
using WinUISnippingTool.Helpers;
using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Windows.Services.Maps;
using CommunityToolkit.Mvvm.DependencyInjection;
using Windows.Devices.PointOfService.Provider;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using System.Numerics;
using SharpDX;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.Foundation;
using Microsoft.UI.Composition;


namespace WinUISnippingTool.ViewModels;

public sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private readonly Dictionary<string, NotifyOnCompletionCollection<UIElement>> shapesDictionary;
    private readonly Dictionary<string, Microsoft.UI.Xaml.Controls.Image> imagesDictionary;
    private readonly Dictionary<string, SoftwareBitmap> softwareBitmaps;

    private readonly SnipPaintBase windowPaint;
    private readonly SnipPaintBase customShapePaint;
    private readonly SnipPaintBase rectangleSelectionPaint;
    private readonly WindowPaint windowPaintSource;
    private SnipPaintBase paintSnipKind;

    private string currentMonitorName;
    public MonitorLocation PrimaryMonitor { get; private set; }

    public event Func<Task> OnExitFromWindow;
    public Shape ResultFigure { get; private set; }
    public int ResultFigureActualWidth { get; private set; }
    public int ResultFigureActualHeight { get; private set; }

    public ImageSource CurrentShapeBmp { get; private set; }
    public RectInt32 VideoFramePosition { get; private set; }

    public string CurrentMonitorName => currentMonitorName;
    public bool IsShortcutResponce { get; private set; }
    public bool CompleteRendering { get; private set; }

    public void SetCurrentMonitor(string monitorName) => currentMonitorName = monitorName;

    public void TrySetAndLoadLocalization(string bcpTag) => LoadLocalization(bcpTag);

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
        PrimaryMonitor = monitorLocation;
    }

    public SnipScreenWindowViewModel() : base()
    {
        LoadLocalization(CoreConstants.DefaultLocalizationBcp);
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
        CompleteRendering = false;
        IsOverlayVisible = true;
        CurrentShapeBmp = null;
    }

    public void AddSoftwareBitmapForCurrentMonitor(MonitorLocation location, SoftwareBitmap bitmap)
    {
        softwareBitmaps.Add(location.DeviceName, bitmap);
    }

    public void SetShapeSourceForCurrentMonitor()
    {
        var canvasItems = shapesDictionary[currentMonitorName];

        windowPaintSource.SetShapeSource(canvasItems);
        customShapePaint.SetShapeSource(canvasItems);
        rectangleSelectionPaint.SetShapeSource(canvasItems);
    }

    public void AddImageSourceAndBrushFillForCurentMonitor(ImageSource source)
    {
        var canvasItems = shapesDictionary[currentMonitorName];

        var image = imagesDictionary[currentMonitorName];
        image.Source = source;
        image.Opacity = 0.3;
        canvasItems.Add(image);
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

    public void SetSelectedItem(SnipShapeKind selectedKind, CaptureType selectedCapturType)
    {
        SelectedSnipKind = selectedKind;
        CaptureType = selectedCapturType;
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
        var saveToFolderTask = FolderExtensions.SaveBitmapAsync(softwareBitmap)
                    .ContinueWith(t =>
                    {
                        if (IsShortcutResponce)
                        {
                            var imageUri = new Uri($"file:///{t.Result.Path}");
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

        var clipboardTask = ClipboardExtensions.CopyAsync(softwareBitmap);

        return Task.WhenAll(saveToFolderTask, clipboardTask);
    }

    private async Task<SoftwareBitmap> GetSingleMonitorSnapshot()
    {
        SoftwareBitmap result;

        if(SelectedSnipKind.Kind == SnipKinds.CustomShape)
        {
            var figure = (Polyline)ResultFigure;
            var softwarebitmap = softwareBitmaps[currentMonitorName];

            result = await RenderExtensions.ProcessPointsAsync(figure.Points, softwarebitmap);
        }
        else
        {
            result = await RenderExtensions.ProcessShapeAsync(ResultFigure);
        }

        paintSnipKind.Clear();

        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(result);
        CurrentShapeBmp = source;
        ResultFigureActualWidth = result.PixelWidth;
        ResultFigureActualHeight = result.PixelHeight;

        return result;
    }

    private async Task<SoftwareBitmap> GetAllMonitorsSnapshot()
    {
        var images = softwareBitmaps.Values.AsEnumerable();
        var softwareBitmap = await RenderExtensions.ProcessImagesAsync(PrimaryMonitor, images);

        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(softwareBitmap);

        CurrentShapeBmp = source;

        ResultFigureActualWidth = softwareBitmap.PixelWidth;
        ResultFigureActualHeight = softwareBitmap.PixelHeight;
        return softwareBitmap;
    }

    public async Task OnPointerReleased(Windows.Foundation.Point position)
    {
        if (paintSnipKind is not null && !CompleteRendering)
        {
            ResultFigure = paintSnipKind.OnPointerReleased(position);

            if (ResultFigure is not null)
            {
                if (CaptureType == CaptureType.Photo
                    && SelectedSnipKind.Kind != SnipKinds.AllWindows)
                {
                    var sbitmap = await GetSingleMonitorSnapshot();
                    await CreateSaveBmpToAllPlacesTask(sbitmap);
                }
                else if (CaptureType == CaptureType.Video)
                {
                    var startX = (int)ResultFigure.ActualOffset.X;
                    var startY = (int)ResultFigure.ActualOffset.Y;
                    var width = paintSnipKind.ActualSize.Width;
                    var height = paintSnipKind.ActualSize.Height;

                    if (width < 0)
                    {
                        startX += width;
                        width = Math.Abs(width);
                    }

                    if (height < 0)
                    {
                        startY += height;
                        height = Math.Abs(height);
                    }

                    VideoFramePosition = new(startX, startY, width, height);
                }

                CompleteRendering = true;
            }
        }
        else if (SelectedSnipKind.Kind == SnipKinds.AllWindows)
        {
            var sbitmap = await GetAllMonitorsSnapshot();
            _ = Task.Run(() => Console.Beep(500, 200));
            await CreateSaveBmpToAllPlacesTask(sbitmap);

            CompleteRendering = true;
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

    public async Task ExitAsync()
    {
        foreach (var item in imagesDictionary)
        {
            var softwareBitmapSource = (SoftwareBitmapSource)item.Value.Source;
            softwareBitmapSource.Dispose();
            item.Value.Source = null;
        }

        foreach (var softwareBitmap in softwareBitmaps.Values)
        {
            softwareBitmap.Dispose();
        }

        softwareBitmaps.Clear();
        imagesDictionary.Clear();
        shapesDictionary.Clear();

        if (IsShortcutResponce)
        {
            CurrentShapeBmp = null;
        }

        if (OnExitFromWindow is not null)
        {
            await OnExitFromWindow();
        }
    }

    public async Task TryExitAsync()
    {
        if (CompleteRendering)
        {
            await ExitAsync();
        }
        else
        {
            IsOverlayVisible = true;
        }
    }

    internal void PrepareModel(MonitorLocation location, SoftwareBitmap softwareBitmap, SoftwareBitmapSource softwareBitmapSource, SnipShapeKind selectedSnipKind, CaptureType captureType, bool byShortcut)
    {
        SetCurrentMonitor(location.DeviceName);
        _ = GetOrAddCollectionForCurrentMonitor();
        AddImageSourceAndBrushFillForCurentMonitor(softwareBitmapSource);
        AddSoftwareBitmapForCurrentMonitor(location, softwareBitmap);
        AddShapeSourceForCurrentMonitor();
        SetWindowSize(location.MonitorSize);
        SetResponceType(byShortcut);
        SetImageSourceForCurrentMonitor();
        SetSelectedItem(selectedSnipKind, captureType);
    }
}
