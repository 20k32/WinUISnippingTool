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


namespace WinUISnippingTool.ViewModels;

internal sealed class SnipScreenWindowViewModel : CanvasViewModelBase
{
    private readonly Dictionary<string, NotifyOnCompletionCollection<UIElement>> shapesDictionary;
    private readonly Dictionary<string, Microsoft.UI.Xaml.Controls.Image> imagesDictionary;
    private readonly Dictionary<MonitorLocation, SoftwareBitmap> softwareBitmaps;

    private readonly SnipPaintBase windowPaint;
    private readonly SnipPaintBase customShapePaint;
    private readonly SnipPaintBase rectangleSelectionPaint;
    private readonly WindowPaint windowPaintSource;
    private SnipPaintBase paintSnipKind;

    private string currentMonitorName;
    private bool shortcutResponce;
    private MonitorLocation primaryMonitor;

    public event Action OnExitFromWindow;
    public bool ExitRequested;
    public Shape ResultFigure { get; private set; }
    public int ResultFigureActualWidth;
    public int ResultFigureActualHeight;
    public ImageSource CurrentShapeBmp;

    public string CurrentMonitorName => currentMonitorName;
    public void SetCurrentMonitor(string monitorName) => currentMonitorName = monitorName;
    
    public override void SetWindowSize(Windows.Foundation.Size newSize)
    {
        base.SetWindowSize(newSize);
        windowPaintSource.SetWindowSize(newSize);
    }

    public void SetResponceType(bool isShortcut)
    {
        shortcutResponce = isShortcut;
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

    private Task CreateSaveBmpToAllPlacesTask(uint width, uint height, byte[] buffer)
    {
        var saveToFileTask = PicturesFolderExtensions.SaveAsync(width, height, buffer)
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

                            var notification = builder.BuildNotification();
                            var notificationManager = AppNotificationManager.Default;
                            notificationManager.Show(notification);
                        }
                    });

        var saveToClipboardTask = ClipboardExtensions.CopyAsync(width, height, buffer);

        return Task.WhenAll(saveToFileTask, saveToClipboardTask);
    }

    private async Task<byte[]> GetSingleMonitorSnapshot()
    {
        var renderTargetBitmap = new RenderTargetBitmap();
        CurrentShapeBmp = renderTargetBitmap;

        await renderTargetBitmap.RenderAsync(ResultFigure);

        paintSnipKind.Clear();

        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
        ResultFigureActualWidth = renderTargetBitmap.PixelWidth;
        ResultFigureActualHeight = renderTargetBitmap.PixelHeight;
        var pixels = pixelBuffer.ToArray();

        if (shortcutResponce)
        {
            CurrentShapeBmp = null;
        }

        return pixels;
    }

    private async Task<byte[]> GetAllMonitorsSnapshot()
    {
        int height = softwareBitmaps.Values.First().PixelHeight;
        int width = softwareBitmaps.Values.First().PixelWidth;

        foreach (var item in softwareBitmaps.Values.Skip(1))
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

        CanvasDevice device = CanvasDevice.GetSharedDevice();
        CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)newWidth, (int)newHeight, 96);
        
        using (var drawingSession = renderTarget.CreateDrawingSession())
        {
            drawingSession.Clear(Colors.Transparent);

            var firstBmp = softwareBitmaps.Values.Last();

            var firstImage = CanvasBitmap.CreateFromSoftwareBitmap(device, firstBmp);
            var newBoundWidth = (float)(firstImage.Bounds.Width * minScale);
            var newBoundHeight = (float)(firstImage.Bounds.Height * minScale);

            var matrix = System.Numerics.Matrix3x2.CreateScale((float)minScale, (float)minScale);
            drawingSession.Transform = matrix;

            drawingSession.DrawImage(firstImage, 0, 0);

            var prevBoundWidth = firstImage.Bounds.Width;

            foreach (var bitmap in softwareBitmaps.Values.Reverse().Skip(1))
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
            await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg);

            var bmp = new BitmapImage();
            stream.Seek(0);
            await bmp.SetSourceAsync(stream);

            CurrentShapeBmp = bmp;
        }

        ResultFigureActualWidth = (int)renderTarget.Size.Width;
        ResultFigureActualHeight = (int)renderTarget.SizeInPixels.Height;

        var pixelBytes = renderTarget.GetPixelBytes();

        return pixelBytes;
    }

    public async Task OnPointerReleased(Windows.Foundation.Point position)
    {
        byte[] pixels = Array.Empty<byte>();

        if(paintSnipKind is not null)
        {
            ResultFigure = paintSnipKind.OnPointerReleased(position);
            
            if (ResultFigure is not null
                && SelectedSnipKind.Kind != SnipKinds.AllWindows)
            {
                pixels = await GetSingleMonitorSnapshot();
                await CreateSaveBmpToAllPlacesTask((uint)ResultFigureActualWidth, (uint)ResultFigureActualHeight, pixels);
            }
        }
        else if (SelectedSnipKind.Kind == SnipKinds.AllWindows)
        {
            pixels = await GetAllMonitorsSnapshot();
            Task.Run(() => Console.Beep(500, 200));
            await CreateSaveBmpToAllPlacesTask((uint)ResultFigureActualWidth, (uint)ResultFigureActualHeight, pixels);
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
            var softwareBitmapSource = (SoftwareBitmapSource)item.Value.Source;
            softwareBitmapSource.Dispose();
            item.Value.Source = null;
        }

        foreach(var softwareBitmap in softwareBitmaps.Values)
        {
            softwareBitmap.Dispose();
        }

        softwareBitmaps.Clear();

        OnExitFromWindow?.Invoke();
    }

    internal void AddTest()
    {
        SnipShapeKinds.Add(new("asdfasdf", "sdaf", SnipKinds.Recntangular));
    }
}
