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

namespace WinUISnippingTool.ViewModels;

internal sealed partial class MainWindowViewModel : CanvasViewModelBase
{
    private DrawBase drawBrush;
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
        TrySetAndLoadLocalization("uk-UA");
        previousImageExists = false;

        drawBrushColor = new SolidColorBrush(Colors.Yellow);
        drawBrushThickness = 3;
        drawBrush = new SimpleBrush(CanvasItems, drawBrushColor, drawBrushThickness);
        SelectedSnipKind = SnipShapeKinds.First();
    }

    public void OnPointerPressed(Point value) => drawBrush.OnPointerPressed(value);
    public void OnPointerMoved(Point value) => drawBrush.OnPointerMoved(value);
    public void OnPointerReleased(Point value) => drawBrush.OnPointerReleased(value);

    public Size GetActualImageSize() => transformManager.ActualSize;

    [RelayCommand]
    private void SetEraseBrush()
    {
        drawBrush = new Erase(CanvasItems);
    }

    [RelayCommand]
    private void GlobalUndo()
    {
        drawBrush.UndoGlobal();
    }

    [RelayCommand]
    private void GlobalRedo()
    {
        drawBrush.RedoGlobal();
    }

    [RelayCommand]
    private void SetSimpleBrush()
    {
        drawBrush = new SimpleBrush(CanvasItems, drawBrushColor, drawBrushThickness);
    }

    [RelayCommand]
    private void SetMarkerBrush()
    {
        drawBrush = new MarkerBrush(CanvasItems, drawBrushColor, drawBrushThickness);
    }

    [RelayCommand]
    private void ResetCanvas()
    {
        drawBrush.Clear();
    }

    public async Task SaveBmpToFileAsync (RenderTargetBitmap renderBitmap)
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
        await ClipboardExtensions.CopyAsync((uint)renderBitmap.PixelWidth, (uint)renderBitmap.PixelHeight, pixelBuffer.ToArray());
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
                drawBrush.Clear();
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
}
