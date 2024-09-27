using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Views;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel.DataTransfer;
using System;
using WinUISnippingTool.Models.Draw;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Windows.Foundation;
using WinUISnippingTool.Models.Extensions;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using CommunityToolkit.WinUI.UI.Controls;
using System.IO;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace WinUISnippingTool.ViewModels;

internal sealed partial class MainWindowViewModel : CanvasViewModelBase
{
    private DrawBase tempBrush;
    private DrawBase drawBrush;
    private DrawBase simpleBrush;
    private DrawBase eraseBrush;
    private DrawBase markerBrush;
    private SolidColorBrush drawBrushColor;
    private double drawBrushThickness;
    private SnipScreenWindow snipScreen;
    private bool previousImageExists;
    private ScaleTransformManager transformManager;


    public MainWindowViewModel() : base()
    {
        transformManager = new();
        CanvasWidth = 100;
        CanvasHeight = 100;
        previousImageExists = false;
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
        TrySetAndLoadLocalization("uk-UA");
        MarkerColorList = new();
        MarkerColorList.AddRange(new ColorKind[]
        {
            new("#98F5F9"),
            new("#EFC3CA"),
            new("#FFECA1")
        });

        simpleBrush = new SimpleBrush(CanvasItems);
        markerBrush = new MarkerBrush(CanvasItems);
        eraseBrush = new EraseBrush(CanvasItems);

        DrawingStrokeThickness = 1;
        MarkerStrokeThickness = 0.5;
        IsSnapshotTaken = false;
    }

    protected override void TrySetAndLoadLocalization(string bcpTag)
    {
        base.TrySetAndLoadLocalization(bcpTag);
        TakePhotoButtonName = resourceMap.GetValue("TakePhotoButtonName/Text")?.ValueAsString ?? "emtpy_value";
    }

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
            if(drawBrush is EraseBrush)
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
        if(CanvasItems.Count == 1)
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
        drawBrush?.Clear();
        GlobalUndoCommand.NotifyCanExecuteChanged();
        GlobalRedoCommand.NotifyCanExecuteChanged();
    }

    public async Task SaveBmpToFileAsync(RenderTargetBitmap renderBitmap)
    {
        var pixelBuffer = await renderBitmap.GetPixelsAsync();
        var file = await FilePickerExtensions.ShowSaveAsync();

        if (file is not null)
        {
            await FileExtensions.SaveBmpBufferAsync(
            file,
            (uint)renderBitmap.PixelWidth,
            (uint)renderBitmap.PixelHeight,
            pixelBuffer.ToArray());
        }
    }

    public async Task SaveBmpToClipboardAsync(RenderTargetBitmap renderBitmap)
    {
        var pixelBuffer = await renderBitmap.GetPixelsAsync();

        await ClipboardExtensions.CopyAsync(
            (uint)renderBitmap.PixelWidth, 
            (uint)renderBitmap.PixelHeight, 
            pixelBuffer.ToArray());
    }

    public ScaleTransform TransformSource => transformManager.TransfromSource;

    public void EnterSnippingMode(bool byShortcut, Action sizeChangedCallback = null)
    {
        var bitmapImage = ScreenshotHelper.GetBitmapImageScreenshotForArea(defaultWindowSize);
        snipScreen = new();
        snipScreen.ViewModel.SetWindowSize(defaultWindowSize);
        snipScreen.ViewModel.SetBitmapImage(bitmapImage);
        snipScreen.ViewModel.SetResponceType(byShortcut);
        snipScreen.ViewModel.SetSelectedItem(SelectedSnipKind.Kind);

        snipScreen.Closed += async (x, args) =>
        {
            if (!snipScreen.ViewModel.ExitRequested)
            {
                drawBrush?.Clear();
                CanvasItems.Clear();
                var vm = ((SnipScreenWindowViewModel)snipScreen.ViewModel);
                var content = Clipboard.GetContent();
                var bitmap = await content.GetBitmapAsync();
                using (var stream = await bitmap.OpenReadAsync())
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(stream);
                    CanvasItems.Add(new Image { Source = bitmapImage });
                }

                CanvasWidth = vm.ResultFigureActualWidth;
                CanvasHeight = vm.ResultFigureActualHeight;

                SetTransformObjectSize(new(CanvasWidth, CanvasHeight));
                SetScaleCenterCoords(new(CanvasWidth, CanvasHeight));

                IsSnapshotTaken = true;
                sizeChangedCallback?.Invoke();
            }
        };

        snipScreen.PrepareWindow();
        snipScreen.Activate();
    }

    public void AddImage(Image image, int width, int height)
    {
        if (previousImageExists)
        {
            var existing = (Image)CanvasItems.First(x => x.GetType() == typeof(Image));
            CanvasItems.Remove(existing);
            previousImageExists = false;
        }
        else
        {
            previousImageExists = true;
        }
        CanvasItems.Add(image);
        var bitmapImage = ((BitmapImage)image.Source);

        CanvasWidth = width;
        CanvasHeight = height;

        SetTransformObjectSize(new(CanvasWidth, CanvasHeight));
        SetScaleCenterCoords(new(CanvasWidth, CanvasHeight));
    }

    public void SetTransformObjectSize(Windows.Foundation.Size transformObject)
        => transformManager.SetTransformObject(transformObject);

    public void SetRelativeObjectSize(Windows.Foundation.Size relativeObject)
        => transformManager.SetRelativeObject(relativeObject);

    public void SetScaleCenterCoords(Windows.Foundation.Size size)
        => transformManager.SetScaleCenterCoords(size);

    public void Transform(Windows.Foundation.Size relativeTo) => transformManager.Transform(relativeTo);
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
            if(isSnapshotTaken != value)
            {
                isSnapshotTaken = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion


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
            await stream.WriteAsync(pixelsArr, 0, pixelsArr.Length);
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

    public void ExitCroppingMode()
    {
        IsInCroppingMode = false;
        CanvasItems.Remove(imageCropper);

        drawBrush = tempBrush;
        tempBrush = null;
    }
}
