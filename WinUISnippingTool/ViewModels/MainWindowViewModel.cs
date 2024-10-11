using System.Linq;
using System.Threading.Tasks;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Views;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using WinUISnippingTool.Models.Draw;
using CommunityToolkit.Mvvm.Input;
using Windows.Foundation;
using WinUISnippingTool.Models.Extensions;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.WinUI.UI.Controls;
using System.IO;
using Windows.ApplicationModel.Resources.Core;
using System.Collections.Generic;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using WinUISnippingTool.Views.UserControls;
using WinUISnippingTool.Models.VideoCapture;
using Windows.Media.Devices;
using System.Globalization;
using Windows.Media.Core;
using Microsoft.UI;
using Windows.Storage;
using System.Diagnostics;
namespace WinUISnippingTool.ViewModels;


internal sealed partial class MainWindowViewModel : CanvasViewModelBase
{
    private readonly DrawBase simpleBrush;
    private readonly DrawBase eraseBrush;
    private readonly DrawBase markerBrush;
    private readonly SnipScreenWindowViewModel snipScreenWindowViewModel;
    private readonly VideoCaptureWindowViewModel videoCaptureWindowViewModel;
    private readonly ScaleTransformManager transformManager;
    private readonly List<MonitorLocation> monitorLocations;
    private readonly List<SnipScreenWindow> snipScreenWindows;

    private DrawBase tempBrush;
    private DrawBase drawBrush;

    public NotifyOnCompletionCollection<UIElement> CanvasItems { get; private set; }
    public string BcpTag { get; private set; }

    public event Action OnNewImageAdded;
    public event Action OnSnippingModeEntered;
    public event Action<bool> OnSnippingModeExited;
    public event Action OnVideoModeEntered;
    public event Action<bool> OnVideoModeExited;

    private bool byShortcut;

    public MainWindowViewModel() : base()
    {
        snipScreenWindows = new();
        snipScreenWindowViewModel = new();
        snipScreenWindowViewModel.OnExitFromWindow += OnExitFromWindow;

        videoCaptureWindowViewModel = new();

        CanvasItems = new();
        monitorLocations = new();
        transformManager = new();
        CanvasWidth = 100;
        CanvasHeight = 100;
        
        SelectedSnipKind = SnipShapeKinds.First();
        DrawingColorList = new();

        DrawingColorList.AddRange(new ColorKind[]
        {
            new("#000000"),
            new("#E8E8E8"),
            new("#D20103"),
            new("#FE9900"),
            new("#7DDA58"),
            new("#5DE2E7"),
            new("#060270"),
            new("#8D6F64"),
            new("#CC6CE7")
        });

        LoadLocalization(CoreConstants.DefaultLocalizationBcp);

        MarkerColorList = new();
        MarkerColorList.AddRange(new ColorKind[]
        {
            new("#98F5F9"),
            new("#EFC3CA"),
            new("#FFECA1")
        });

        simpleBrush = new SimpleBrush(CanvasItems);
        markerBrush = new MarkerBrush(CanvasItems);
        eraseBrush = new EraseBrush(CanvasItems, GlobalUndoCommand.NotifyCanExecuteChanged);

        DrawingStrokeThickness = 1;
        MarkerStrokeThickness = 0.5;
        IsSnapshotTaken = false;

        byShortcut = false;
    }

    public void UnregisterHandlers()
    {
        snipScreenWindowViewModel.OnExitFromWindow -= OnExitFromWindow;
    }

    private async void OnExitFromWindow()
    {
        foreach (var window in snipScreenWindows)
        {
            window.Close();
        }

        snipScreenWindows.Clear();

        OnSnippingModeExited?.Invoke(byShortcut);

        if (snipScreenWindowViewModel.CompleteRendering)
        {
            if (SnipControl.CaptureKind == CaptureType.Photo)
            {
                AddImageCore();
            }
            else if (SnipControl.CaptureKind == CaptureType.Video)
            {
                OnVideoModeEntered?.Invoke();
                await ShowVideoCaptureScreenAsync();
            }
        }
    }

    public void AddMonitorLocation(MonitorLocation location)
        => monitorLocations.Add(location);

    protected override void LoadLocalization(string bcpTag)
    {
        if (bcpTag != BcpTag)
        {
            base.LoadLocalization(bcpTag);
            BcpTag = bcpTag;
            TakePhotoButtonName = ResourceMap.GetValue("TakePhotoButtonName/Text")?.ValueAsString ?? "emtpy_value";
            SettingsButtonName = ResourceMap.GetValue("Settings/Text")?.ValueAsString ?? "empty_value";

            snipScreenWindowViewModel.TrySetAndLoadLocalization(bcpTag);
        }
    }

    public void TrySetAndLoadLocalizationWrapper(string bcpTag) => LoadLocalization(bcpTag);

    public void OnPointerPressed(Point value) => drawBrush?.OnPointerPressed(value);

    public void OnPointerMoved(Point value)
    {
        if (drawBrush is not null
            && value.X > 0
            && value.Y > 0
            && value.X < CanvasWidth
            && value.Y < CanvasHeight)
        {
            drawBrush.OnPointerMoved(value);
        }
    }
    public void OnPointerReleased(Point value)
    {
        if (drawBrush is not null)
        {
            drawBrush.OnPointerReleased(value);

            if (drawBrush is EraseBrush)
            {
                GlobalRedoCommand.NotifyCanExecuteChanged();
            }
            GlobalUndoCommand.NotifyCanExecuteChanged();
        }
    }

    public Size GetActualImageSize() => transformManager.ActualSize;

    [RelayCommand]
    private void SetEraseBrush()
    {
        drawBrush = eraseBrush;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void GlobalUndo()
    {
        drawBrush.UndoGlobal();
        if (CanvasItems.Count == 1)
        {
            GlobalUndoCommand.NotifyCanExecuteChanged();
        }

        GlobalRedoCommand.NotifyCanExecuteChanged();
    }

    private bool CanUndo() => drawBrush is not null && CanvasItems.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void GlobalRedo()
    {
        drawBrush.RedoGlobal();

        if (!drawBrush.CanRedo())
        {
            GlobalRedoCommand.NotifyCanExecuteChanged();
        }

        GlobalUndoCommand.NotifyCanExecuteChanged();
    }

    private bool CanRedo() => drawBrush is not null && drawBrush.CanRedo();

    [RelayCommand]
    private void SetSimpleBrush()
    {
        drawBrush = simpleBrush;
    }

    [RelayCommand]
    private void SetMarkerBrush()
    {
        drawBrush = markerBrush;
    }

    [RelayCommand]
    private void ResetCanvas()
    {
        ClearCanvasCore();
    }

    public async Task SaveVideoAsync()
    {
        var file = await FilePickerExtensions.ShowSaveVideoAsync();

        if (file is not null)
        {
            var uri = videoCaptureWindowViewModel.GetVideoUri();
            var exisitngVideoFile = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
            await exisitngVideoFile.CopyAndReplaceAsync(file);
        }
    }

    public async Task SaveBitmapAsync(RenderTargetBitmap renderBitmap)
    {
        var file = await FilePickerExtensions.ShowSaveImageAsync();

        var pixelBuffer = await renderBitmap.GetPixelsAsync();

        if (file is not null)
        {
            await FileExtensions.SaveBmpBufferAsync(
            file,
            (uint)renderBitmap.PixelWidth,
            (uint)renderBitmap.PixelHeight,
            pixelBuffer.ToArray());
        }
    }

    public bool CanShowWindow =>
        !byShortcut
            && (SnipControl.CaptureKind == CaptureType.Photo
                || !snipScreenWindowViewModel.CompleteRendering);

    public bool CanMinimizeWindow => 
        byShortcut 
            && (SnipControl.CaptureKind == CaptureType.Photo
                || !snipScreenWindowViewModel.CompleteRendering);


    /// <returns>true - image, false - video</returns>
    public bool? IsDrawingElementTypeOfImage()
    {
        bool? result = null;

        if(CanvasItems.Count > 0)
        {
            if (CanvasItems[0] is Image)
            {
                result = true;
            }
            else if (CanvasItems[0] is MediaPlayerElement)
            {
                result = false;
            }
        }

        return result;
    }

    public async Task SaveBmpToClipboardAsync(RenderTargetBitmap renderBitmap)
    {
        var pixelBuffer = await renderBitmap.GetPixelsAsync();

        await ClipboardExtensions.CopyAsync(
            (uint)renderBitmap.PixelWidth,
            (uint)renderBitmap.PixelHeight,
            pixelBuffer.ToArray());
    }

    private void TryAddImageToCanvas(ImageSource source, double width, double height)
    {
        if (CanvasItems.Count > 0 && CanvasItems[0] is Image image)
        {
            image.Source = source;

            if(image.Clip is not null)
            {
                image.Clip.Rect = new Rect(0, 0, width, height);
            }
        }
        else
        {
            CanvasItems.Clear();
            CanvasItems.Add(new Image { Source = source });
        }
    }

    private void TryAddMediaPlayerToCanvas(Uri source)
    {
        var mediaSource = MediaSource.CreateFromUri(source);
        MediaPlayerElement mediaPlayer;

        if (CanvasItems.Count > 0 && CanvasItems[0]
            is MediaPlayerElement existingPlayer)
        {
            mediaPlayer = existingPlayer;
        }
        else
        {
            mediaPlayer = new MediaPlayerElement();
            mediaPlayer.AreTransportControlsEnabled = true;
            mediaPlayer.TransportControls.IsCompact = true;
            mediaPlayer.TransportControls.Width = 400;
            mediaPlayer.TransportControls.IsVolumeEnabled = false;
            mediaPlayer.TransportControls.IsVolumeButtonVisible = false;
            mediaPlayer.Source = mediaSource;

            mediaPlayer.Stretch = Stretch.UniformToFill;

            mediaPlayer.HorizontalContentAlignment = HorizontalAlignment.Center;
            mediaPlayer.VerticalContentAlignment = VerticalAlignment.Center;

        }

        mediaPlayer.Source = mediaSource;
        var tempWidth = snipScreenWindowViewModel.VideoFramePosition.Width;
        
        mediaPlayer.Width = Math.Max(CoreConstants.DefaultVideoPlayerWidth, tempWidth);
        mediaPlayer.Height = snipScreenWindowViewModel.VideoFramePosition.Height;

        CanvasItems.Clear();
        CanvasItems.Add(mediaPlayer);

        CanvasWidth = mediaPlayer.Width;
        CanvasHeight = mediaPlayer.Height;

        SetTransformObjectSize(new(CanvasWidth, CanvasHeight));
        SetScaleCenterCoords(new(CanvasWidth, CanvasHeight));
    }

    private void ClearCanvasCore()
    {
        if (drawBrush is not null)
        {
            drawBrush.Clear();
            GlobalUndoCommand.NotifyCanExecuteChanged();
            GlobalRedoCommand.NotifyCanExecuteChanged();
        }
        else
        {
            var array = CanvasItems.Skip(1).ToArray(); // 1st element is image or mediaplayer
            CanvasItems.RemoveRange(array);
        }
    }

    public ScaleTransform TransformSource => transformManager.TransfromSource;

    public void AddImageFromSource(ImageSource source, double width, double height)
    {
        ClearCanvasCore();

        TryAddImageToCanvas(source, width, height);

        CanvasWidth = width;
        CanvasHeight = height;

        SetTransformObjectSize(new(CanvasWidth, CanvasHeight));
        SetScaleCenterCoords(new(CanvasWidth, CanvasHeight));

        IsSnapshotTaken = true;
        IsImageLoaded = true;

        OnNewImageAdded?.Invoke();
    }

    private void AddMediaPlayer(Uri videoUri)
    {
        ClearCanvasCore();

        TryAddMediaPlayerToCanvas(videoUri);

        IsSnapshotTaken = true;
        IsImageLoaded = false;

        OnNewImageAdded?.Invoke();
    }

    private async Task ShowVideoCaptureScreenAsync()
    {
        var framePosition = snipScreenWindowViewModel.VideoFramePosition;
        var currentMonitor = monitorLocations.First(monitor => monitor.DeviceName == snipScreenWindowViewModel.CurrentMonitorName);

        videoCaptureWindowViewModel.SetMonitorForCapturing(currentMonitor);
        videoCaptureWindowViewModel.SetCaptureSize((uint)currentMonitor.MonitorSize.Width, (uint)currentMonitor.MonitorSize.Height);
        videoCaptureWindowViewModel.SetFrameForMonitor(framePosition);

        var videoCaptureWindow = new VideoCaptureWindow(currentMonitor, videoCaptureWindowViewModel);

        videoCaptureWindow.PrepareWindow();

        await videoCaptureWindow.ActivateAndStartCaptureAsync();

        if (videoCaptureWindow.Exited)
        {
            var uri = videoCaptureWindowViewModel.GetVideoUri();

            OnVideoModeExited?.Invoke(byShortcut);

            AddMediaPlayer(uri);
        }
    }

    public void AddImageCore()
    {
        if (!snipScreenWindowViewModel.IsShortcutResponce)
        {
            AddImageFromSource(snipScreenWindowViewModel.CurrentShapeBmp,
                snipScreenWindowViewModel.ResultFigureActualWidth,
                snipScreenWindowViewModel.ResultFigureActualHeight);
        }
    }

    public async Task EnterSnippingModeAsync(bool byShortcut)
    {
        this.byShortcut = byShortcut;

        OnSnippingModeEntered?.Invoke();

        foreach (var location in monitorLocations)
        {
            var window = new SnipScreenWindow();
            await window.PrepareWindowAsync(snipScreenWindowViewModel, location, SelectedSnipKind.Kind, byShortcut);
            snipScreenWindows.Add(window);
        }

        foreach (var item in snipScreenWindows)
        {
            item.Activate();
        }
    }

    public void SetTransformObjectSize(Size transformObject)
        => transformManager.SetTransformObject(transformObject);

    public void SetRelativeObjectSize(Size relativeObject)
        => transformManager.SetRelativeObject(relativeObject);

    public void SetScaleCenterCoords(Size size)
        => transformManager.SetScaleCenterCoords(size);

    public void Transform(Size relativeTo) => transformManager.Transform(relativeTo);
    public void Transform() => transformManager.Transform();
    public void ResetTransform() => transformManager.ResetTransform();

    #region Take photo button name

    private string takePhotoButtonName;
    public string TakePhotoButtonName
    {
        get => takePhotoButtonName;
        set
        {
            if (takePhotoButtonName != value)
            {
                takePhotoButtonName = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion

    #region Drawing Color list

    public NotifyOnCompletionCollection<ColorKind> DrawingColorList { get; }

    private ColorKind selectedDrawingColor;

    public ColorKind SelectedDrawingColor
    {
        get => selectedDrawingColor;
        set
        {
            if (selectedDrawingColor != value)
            {
                selectedDrawingColor = value;
                NotifyOfPropertyChange();
                simpleBrush?.SetColorHex(selectedDrawingColor.Hex);
            }
        }
    }

    #endregion

    #region Marker color list

    public NotifyOnCompletionCollection<ColorKind> MarkerColorList { get; }

    private ColorKind selectedMarkerColor;

    public ColorKind SelectedMarkerColor
    {
        get => selectedMarkerColor;
        set
        {
            if (selectedMarkerColor != value)
            {
                selectedMarkerColor = value;
                NotifyOfPropertyChange();
                markerBrush?.SetColorHex(selectedMarkerColor.Hex);
            }
        }
    }

    #endregion

    #region Drawing troke thickness

    private double drawingStrokeThickness;

    public double DrawingStrokeThickness
    {
        get => drawingStrokeThickness;
        set
        {
            if (drawingStrokeThickness != value)
            {
                drawingStrokeThickness = value;
                NotifyOfPropertyChange();
                simpleBrush.SetDrawingThickness(drawingStrokeThickness);
            }
        }
    }

    #endregion

    #region Marker stroke thickness

    private double markerStrokeThickness;

    public double MarkerStrokeThickness
    {
        get => markerStrokeThickness;
        set
        {
            if (markerStrokeThickness != value)
            {
                markerStrokeThickness = value;
                NotifyOfPropertyChange();
                markerBrush.SetDrawingThickness(markerStrokeThickness);
            }
        }
    }

    #endregion

    #region Is in cropping mode

    private bool isInCroppingMode;
    public bool IsInCroppingMode
    {
        get => isInCroppingMode;
        set
        {
            isInCroppingMode = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    #region Is snapshot taken

    private bool isSnapshotTaken;
    public bool IsSnapshotTaken
    {
        get => isSnapshotTaken;

        set
        {
            if (isSnapshotTaken != value)
            {
                isSnapshotTaken = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion

    #region Image cropper

    private ImageCropper imageCropper;

    public async Task EnterCroppingMode(RenderTargetBitmap renderTargetBitmap)
    {
        tempBrush = drawBrush;
        drawBrush = null;

        imageCropper = new ImageCropper()
        {
            Padding = new Microsoft.UI.Xaml.Thickness(10),
            ThumbPlacement = ThumbPlacement.Corners,
            SecondaryThumbStyle = null,
            MinCroppedPixelLength = 20,
            MinSelectedLength = 20,
        };

        var pixels = await renderTargetBitmap.GetPixelsAsync();
        var pixelsArr = pixels.ToArray();

        var writeableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);
        using (Stream stream = writeableBitmap.PixelBuffer.AsStream())
        {
            await stream.WriteAsync(pixelsArr);
        }

        imageCropper.Source = writeableBitmap;
        CanvasItems.Add(imageCropper);

        IsInCroppingMode = true;
    }

    public void CommitCrop()
    {
        IsInCroppingMode = false;
        var region = imageCropper.CroppedRegion;
        var image = (Image)CanvasItems[0];
        image.Clip = new RectangleGeometry
        {
            Rect = region
        };


        CanvasItems.Remove(imageCropper);
        drawBrush = tempBrush;
        tempBrush = null;
    }

    public void ExitCroppingMode(bool shouldRestoreBrush)
    {
        IsInCroppingMode = false;
        CanvasItems.Remove(imageCropper);

        if (shouldRestoreBrush)
        {
            drawBrush = tempBrush;
            tempBrush = null;
        }
    }

    #endregion

    #region Settings button name

    private string settingsButtonName;
    public string SettingsButtonName
    {
        get => settingsButtonName;
        set
        {
            if (settingsButtonName != value)
            {
                settingsButtonName = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion

    #region Is video loaded

    private bool isImageLoaded;
    public bool IsImageLoaded
    {
        get => isImageLoaded;
        set
        {
            if(isImageLoaded != value)
            {
                isImageLoaded = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion


    [RelayCommand]
    private void Magnify()
    {
        var newWidth = CanvasWidth + 40;
        var newHeight = CanvasHeight + 50;
        var newSize = new Size(newWidth, newHeight);
        Transform(newSize);
    }
}
