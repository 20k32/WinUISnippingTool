using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.Models.PageParameters;
using WinUISnippingTool.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class MainPage : Page
{
    private DisplayArea display;
    private Microsoft.UI.Windowing.AppWindow currentWindow;
    private bool isScreenTinySized;
    private bool isScreenSmallSized;
    private bool isScreenMiddleSized;
    private bool contentLoaded;
    private Canvas parentCanvas;
    private double PageWidth;
    private double PageHeight;

    private OverlappedPresenter appWindowPersenter;


    public MainPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is MainPageParameter mainPageParameter)
        {
            ViewModel = new();
            appWindowPersenter = mainPageParameter.appWindowPresenter;
            ViewModel = new();
            contentLoaded = true;
            mainGrid.DataContext = ViewModel;
            display = mainPageParameter.displayArea;
            ViewModel.SetWindowSize(new(display.OuterBounds.Width, display.OuterBounds.Height));
        }
    }

    public MainWindowViewModel ViewModel { get; private set; }

    private void ThemeChanged(FrameworkElement sender, object args)
    {
        //throw new NotImplementedException();
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        appWindowPersenter.Minimize(false);
        ViewModel.EnterSnippingMode(false, PART_Canvas_SizeChanged);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        if (contentLoaded)
        {
            PageWidth = args.NewSize.Width;
            PageHeight = args.NewSize.Height - 54;

            if (!isScreenMiddleSized
                 && args.NewSize.Width < CoreConstants.MinLargeWidth)
            {
                PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                isScreenMiddleSized = true;
            }
            else if (!isScreenSmallSized
                     && args.NewSize.Width < CoreConstants.MinMediumWidht)
            {
                PART_MainPane.Children.Remove(PART_RedactPicturePane);
                PART_SubPane.Children.Add(PART_RedactPicturePane);
                PART_TakePictureButtonName.Visibility = Visibility.Visible;
                isScreenSmallSized = true;
            }
            else if (!isScreenTinySized
                    && args.NewSize.Width < CoreConstants.MinSmallWidth)
            {
                PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                isScreenTinySized = true;
            }
            else
            {
                if (isScreenMiddleSized
                         && args.NewSize.Width > CoreConstants.MinLargeWidth)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenMiddleSized = false;
                }
                else if (isScreenSmallSized
                    && args.NewSize.Width > CoreConstants.MinMediumWidht)
                {
                    PART_SubPane.Children.Remove(PART_RedactPicturePane);
                    PART_MainPane.Children.Add(PART_RedactPicturePane);
                    PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                    isScreenSmallSized = false;
                }
                else if (isScreenTinySized
                    && args.NewSize.Width > CoreConstants.MinSmallWidth)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenTinySized = false;
                }
            }
            
            
            PART_Canvas_SizeChanged();
        }
    }

    private void NewPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.EnterSnippingMode(false, PART_Canvas_SizeChanged);

        if (PART_Border.ActualWidth <= PART_Canvas.Width
                   || PART_Border.ActualHeight <= PART_Canvas.Height)
        {
            ViewModel.Transform(new(PART_Border.ActualWidth, PART_Border.ActualHeight));
        }
    }

    public void TransformImage()
    {
        if (PART_Border.ActualWidth <= PART_Canvas.Width
                   || PART_Border.ActualHeight <= PART_Canvas.Height)
        {
            ViewModel.Transform(new(PART_Border.ActualWidth, PART_Border.ActualHeight));
        }
    }

    private void PART_Canvas_SizeChanged()
    {
        if (PageWidth <= ViewModel.CanvasWidth
            || PageHeight - 54 <= ViewModel.CanvasHeight)
        {
            ViewModel.Transform(new(PageWidth, PageHeight - 54));
        }
        else
        {
            ViewModel.ResetTransform();
        }
    }

    private void EnterSnippingModeByShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        appWindowPersenter.Minimize(false);
        ViewModel.EnterSnippingMode(true);
        args.Handled = true;
    }

    private void GlobalUndoShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.GlobalUndoCommand?.Execute(null);
    }

    private void GlobalRedoShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.GlobalRedoCommand?.Execute(null);
    }

    private async void SaveToClipboardShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        var renderBitmap = new RenderTargetBitmap();
        var size = ViewModel.GetActualImageSize();
        await renderBitmap.RenderAsync(PART_Canvas, (int)size.Width, (int)size.Height);
        await ViewModel.SaveBmpToClipboardAsync(renderBitmap);
    }

    private async void SaveFileDialogShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        var renderBitmap = new RenderTargetBitmap();
        var size = ViewModel.GetActualImageSize();
        await renderBitmap.RenderAsync(PART_Canvas, (int)size.Width, (int)size.Height);

        await ViewModel.SaveBmpToFileAsync(renderBitmap);
    }

    private void Canvas_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ViewModel.OnPointerPressed(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void Canvas_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ViewModel.OnPointerMoved(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void Canvas_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void PART_Canvas_Loaded(object sender, RoutedEventArgs e)
    {
        parentCanvas = (Canvas)PART_Canvas.ItemsPanelRoot;
    }

    private async void CropButton_Click(object sender, RoutedEventArgs e)
    {
        var renderBitmap = new RenderTargetBitmap();
        var size = ViewModel.GetActualImageSize();
        await renderBitmap.RenderAsync(PART_Canvas, (int)size.Width, (int)size.Height);
        await ViewModel.EnterCroppingMode(renderBitmap);
    }
    private void DecropButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ExitCroppingMode();
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {

    }
}
