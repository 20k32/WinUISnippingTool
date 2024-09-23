using ABI.Windows.Foundation;
using Microsoft.Graphics.Display;
using Microsoft.UI.Content;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using Windows.Devices.Display;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Services.Maps;
using System.Drawing;
using Windows.UI.WindowManagement;
using WinUISnippingTool.ViewModels;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class MainWindow : Window
    {
        private Microsoft.UI.Windowing.AppWindow currentWindow;
        private bool isScreenTinySized;
        private bool isScreenSmallSized;
        private bool isScreenMiddleSized;
        private bool contentLoaded;

        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            var element = (FrameworkElement)this.Content;
            element.ActualThemeChanged += ThemeChanged;
            ViewModel = new();
            contentLoaded = true;
            //PART_Canvas.RenderTransform = ViewModel.GetTransformSource();
            mainGrid.DataContext = ViewModel;
        }

        private void ThemeChanged(FrameworkElement sender, object args)
        {
            //throw new NotImplementedException();
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {

            //ContentDialog dialog = new ContentDialog();

            //dialog.XamlRoot = this.Content.XamlRoot;
            //dialog.Title = "Save your work?";
            //dialog.PrimaryButtonText = "Save";
            //dialog.SecondaryButtonText = "Don't Save";
            //dialog.CloseButtonText = "Cancel";
            //dialog.DefaultButton = ContentDialogButton.Primary;

            //var result = await dialog.ShowAsync();
            var presenter = ((OverlappedPresenter)AppWindow.Presenter);
            presenter.Minimize(false);
            await Task.Delay(100);
            await ViewModel.EnterSnippingMode();
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

        Microsoft.UI.Xaml.Controls.Image currentImage;

        private void NewPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EnterSnippingMode();
            if (PART_Border.ActualWidth <= PART_Canvas.Width
                       || PART_Border.ActualHeight <= PART_Canvas.Height)
            {
                ViewModel.Transform(new(PART_Border.ActualWidth, PART_Border.ActualHeight));
            }

            //ViewModel.SetScaleCenterCoords(new(PART_Canvas.Width, PART_Canvas.Height));
        }

        private void PART_Canvas_SizeChanged()
        {
            var display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
            
            if (this.Bounds.Width == display.OuterBounds.Width
                    && (this.Bounds.Height == display.OuterBounds.Height - 48
                        || this.Bounds.Height == display.OuterBounds.Height))
            {
                ViewModel.ResetTransform();
            }
            else if (PART_Border.ActualWidth <= ViewModel.CanvasWidth
                || PART_Border.ActualHeight <= ViewModel.CanvasHeight)
            {
                ViewModel.Transform(new(PART_Border.ActualWidth, PART_Border.ActualHeight));
            }
            else if (this.Bounds.Width <= ViewModel.CanvasWidth
                || this.Bounds.Height - 80 <= ViewModel.CanvasHeight)
            {
                ViewModel.Transform(new(this.Bounds.Width, this.Bounds.Height - 80));
            }
            else
            {
                ViewModel.ResetTransform();
            }
        }
    }
}
