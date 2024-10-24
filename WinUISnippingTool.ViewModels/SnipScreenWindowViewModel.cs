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
using System.Collections.Frozen;
using Microsoft.UI.Xaml.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Input;
using Windows.UI.Core;
using WinUISnippingTool.Models.Extensions;
using CommunityToolkit.WinUI.UI;
using System.Runtime.CompilerServices;


namespace WinUISnippingTool.ViewModels;

public sealed partial class SnipScreenWindowViewModel : SnipViewModelBase
{
    private readonly WeakReference<Image> ImageRelativeToWindow;
    private readonly Dictionary<string, NotifyOnCompletionCollection<UIElement>> shapesDictionary;
    private readonly Dictionary<string, Image> imagesDictionary;
    private readonly Dictionary<string, SoftwareBitmap> softwareBitmaps;
    private readonly Dictionary<int, MonitorLocation> locations;

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

    public override void SetWindowSize(Size newSize)
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
        ImageRelativeToWindow = new(null);
        locations = new();
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
        ResultFigure = null;
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

    public Image AddImageSourceAndBrushFillForCurentMonitor(ImageSource source)
    {
        var canvasItems = shapesDictionary[currentMonitorName];

        var image = imagesDictionary[currentMonitorName];
        image.Source = source;
        image.Opacity = 0.3;
        canvasItems.Add(image);

        return image;
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
        customShapePaint.SetImageFill(null!);
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
        if(CaptureType != CaptureType.Video)
        {
            IsPaintListEnabled = true;
            IsPhotoButtonEnabled = false;
            IsVideoButtonEnabled = true;
            SelectedSnipKind = selectedKind;
        }
        else
        {
            SelectedSnipKind = SnipShapeKinds.First();
            IsPaintListEnabled = false;
            IsPhotoButtonEnabled = true;
            IsVideoButtonEnabled = false;
        }

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
        Debug.WriteLine(TimeOnly.FromDateTime(DateTime.Now).Millisecond);
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
            using (var softwarebitmap = softwareBitmaps[currentMonitorName])
            {
                result = await RenderExtensions.ProcessPointsAsync(figure.Points, softwarebitmap);
            }
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
                    using (var softwareBitmap = await GetSingleMonitorSnapshot())
                    {
                        await CreateSaveBmpToAllPlacesTask(softwareBitmap);
                    }
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
            using(var softwareBitmap = await GetAllMonitorsSnapshot())
            {
                _ = Task.Run(() => Console.Beep(500, 200));
                await CreateSaveBmpToAllPlacesTask(softwareBitmap);
            }

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

    [RelayCommand]
    private async Task ExitAsync()
    {
        foreach (var item in imagesDictionary.Values)
        {
            var softwareBitmapSource = (SoftwareBitmapSource)item.Source;
            softwareBitmapSource.Dispose();
            item.Source = null;
        }

        foreach (var softwareBitmap in softwareBitmaps.Values)
        {
            softwareBitmap.Dispose();
        }

        softwareBitmaps.Clear();
        imagesDictionary.Clear();
        shapesDictionary.Clear();
        locations.Clear();

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

    [RelayCommand]
    private void PointerPressed(PointerRoutedEventArgs e)
    {
        e.Handled = true;

        if(e.OriginalSource is Image image)
        {
            ImageRelativeToWindow.SetTarget(image);
        }
        else
        {
            var imageParent = ((UIElement)e.OriginalSource).FindAscendant<Image>();
            ImageRelativeToWindow.SetTarget(imageParent);
        }

        ImageRelativeToWindow.TryGetTarget(out var existingImage);
        var hash = existingImage.GetHashCode();
        var newMonitor = locations[hash];

        if (newMonitor.DeviceName != CurrentMonitorName)
        {
            SetCurrentMonitor(newMonitor.DeviceName);
            SetImageSourceForCurrentMonitor();
            AddShapeSourceForCurrentMonitor();

            var size = WindowExtensions.CalculateDesiredSizeForMonitor(newMonitor);
            SetWindowSize(size);
        }

        var currentLocation = e.GetCurrentPoint(existingImage);

        OnPointerPressed(currentLocation.Position);
    }

    [RelayCommand]
    private void PointerMoved(PointerRoutedEventArgs e)
    {
        e.Handled = true;

        if(ImageRelativeToWindow is not null 
            && ImageRelativeToWindow.TryGetTarget(out var image))
        {
            var currentLocation = e.GetCurrentPoint(image);
            OnPointerMoved(currentLocation.Position);
        }
    }

    [RelayCommand]
    private async Task PointerReleased(PointerRoutedEventArgs e)
    {
        e.Handled = true;

        if (ImageRelativeToWindow is not null 
            && ImageRelativeToWindow.TryGetTarget(out var image)) // user can release mouse when image is not loaded
        {
            var currentLocation = e.GetCurrentPoint(image);

            await OnPointerReleased(currentLocation.Position);

            await TryExitAsync();
        }
    }

    private void AddLocation(int hash, MonitorLocation location) => locations.Add(hash, location);

    internal void PrepareModel(MonitorLocation location, SoftwareBitmap softwareBitmap, SoftwareBitmapSource softwareBitmapSource, SnipShapeKind selectedSnipKind, CaptureType captureType, bool byShortcut)
    {
        SetCurrentMonitor(location.DeviceName);
        _ = GetOrAddCollectionForCurrentMonitor();
        
        var image = AddImageSourceAndBrushFillForCurentMonitor(softwareBitmapSource);
        var hash = image.GetHashCode();
        
        AddLocation(hash, location);
        AddSoftwareBitmapForCurrentMonitor(location, softwareBitmap);
        AddShapeSourceForCurrentMonitor();
        SetWindowSize(location.MonitorSize);
        SetResponceType(byShortcut);
        SetImageSourceForCurrentMonitor();
        SetSelectedItem(selectedSnipKind, captureType);

        CanvasWidth = location.MonitorSize.Width;
        CanvasHeight = location.MonitorSize.Height;
    }
}
