using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.WindowManagement;
using WinUISnippingTool.ViewModels;
using WinUISnippingTool.Models.Extensions;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class MainWindow : Window
    {
        private DisplayArea display;
        private Microsoft.UI.Windowing.AppWindow currentWindow;
        private bool isScreenTinySized;
        private bool isScreenSmallSized;
        private bool isScreenMiddleSized;
        private bool contentLoaded;
        private Canvas parentCanvas;
        

        public MainWindowViewModel ViewModel { get; }
        
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            var element = (FrameworkElement)this.Content;
            element.ActualThemeChanged += ThemeChanged;
            ViewModel = new();
            contentLoaded = true;
            mainGrid.DataContext = ViewModel;
            display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
            ViewModel.SetWindowSize(new(display.OuterBounds.Width, display.OuterBounds.Height));
            nint windowHandle = WindowNative.GetWindowHandle(this);
            FilePickerExtensions.SetWindowHandle(windowHandle);
            
        }

        private void ThemeChanged(FrameworkElement sender, object args)
        {
            //throw new NotImplementedException();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            var presenter = ((OverlappedPresenter)AppWindow.Presenter);
            presenter.Minimize(false);
            ViewModel.EnterSnippingMode(false, PART_Canvas_SizeChanged);
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            if (contentLoaded)
            {
                if (!isScreenMiddleSized
                     && args.Size.Width < 800)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                    isScreenMiddleSized = true;
                }
                else if (!isScreenSmallSized
                         && args.Size.Width < 700)
                {
                    PART_MainPane.Children.Remove(PART_RedactPicturePane);
                    PART_SubPane.Children.Add(PART_RedactPicturePane);
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenSmallSized = true;
                }
                else if (!isScreenTinySized
                        && args.Size.Width < 500)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                    isScreenTinySized = true;
                }
                else
                {
                    if (isScreenMiddleSized
                             && args.Size.Width > 800)
                    {
                        PART_TakePictureButtonName.Visibility = Visibility.Visible;
                        isScreenMiddleSized = false;
                    }
                    else if (isScreenSmallSized
                        && args.Size.Width > 700)
                    {
                        PART_SubPane.Children.Remove(PART_RedactPicturePane);
                        PART_MainPane.Children.Add(PART_RedactPicturePane);
                        PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                        isScreenSmallSized = false;
                    }
                    else if (isScreenTinySized
                        && args.Size.Width > 500)
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
            if (this.Bounds.Width <= ViewModel.CanvasWidth
                || this.Bounds.Height - 80 <= ViewModel.CanvasHeight)
            {
                ViewModel.Transform(new(this.Bounds.Width, this.Bounds.Height - 80));
            }
            else
            {
                ViewModel.ResetTransform();
            }
        }

        private void EnterSnippingModeByShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var presenter = ((OverlappedPresenter)AppWindow.Presenter);
            presenter.Minimize(false);
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
    }
}
