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

            PART_Canvas.RenderTransform = ViewModel.GetTransformSource();
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

            ViewModel.InputCommand();
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
                PART_Canvas_SizeChanged(null, args);
            }
        }

        Microsoft.UI.Xaml.Controls.Image currentImage;

        double tempScaleX;
        double tempScaleY;

        private void NewPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            using (var bmpScreenshot = new Bitmap(500, 500))
            {
                using (var g = Graphics.FromImage(bmpScreenshot))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    g.CopyFromScreen(0, 0, 0, 0, bmpScreenshot.Size);
                    var bitmapImage = new BitmapImage();
                    using (var stream = new MemoryStream())
                    {
                        bmpScreenshot.Save(stream, ImageFormat.Jpeg);
                        stream.Position = 0;
                        bitmapImage.SetSource(stream.AsRandomAccessStream());
                    }
                    PART_Canvas.Width = bitmapImage.PixelHeight;
                    PART_Canvas.Height = bitmapImage.PixelWidth;

                    var child = PART_Canvas.Children.FirstOrDefault();
                    if(child is not null
                        && child is Microsoft.UI.Xaml.Controls.Image imageChild)
                    {
                        PART_Canvas.Children.Remove(imageChild);
                    }

                    var image = new Microsoft.UI.Xaml.Controls.Image();
                    image.Source = bitmapImage;
                    PART_Canvas.Children.Add(image);
                    currentImage = image;
                    ViewModel.SetTransformObjectSize(new(PART_Canvas.Width, PART_Canvas.Height));

                    if (PART_Border.ActualWidth <= PART_Canvas.Width
                        || PART_Border.ActualHeight <= PART_Canvas.Height)
                    {
                        //var scaleX = PART_Border.ActualWidth / 1000;
                        //var scaleY = PART_Border.ActualHeight / 1000;

                        //var scale = Math.Min(scaleX, scaleY);

                        //scaleTransform.ScaleX = scale;
                        //scaleTransform.ScaleY = scale;
                        //ViewModel.SetTransformObjectSize(new(PART_Canvas.Width, PART_Canvas.Height));
                       
                        //ViewModel.SetRelativeObjectSize(new((int)PART_Border.Width, (int)PART_Border.Height));
                        ViewModel.Transform(new((int)PART_Border.ActualWidth, (int)PART_Border.ActualHeight));
                        //scaleTransform.CenterX = PART_Canvas.Width / 2;
                        //scaleTransform.CenterY = PART_Canvas.Height / 2;
                        //ViewModel.SetScaleSenterCoords(new(PART_Canvas.Width, PART_Canvas.Height));
                    }

                    ViewModel.SetScaleSenterCoords(new(PART_Canvas.Width, PART_Canvas.Height));
                }
            }
        }

        //private ScaleTransform scaleTransform = new();

        private void PART_Canvas_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            var display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
            
            if (this.Bounds.Width == display.OuterBounds.Width
                    && this.Bounds.Height == display.OuterBounds.Height - 48)
            {
                //var scaleX = (display.OuterBounds.Width) / 1000;
                //var scaleY = (display.OuterBounds.Height - 82) / 1000;

                //var scale = Math.Min(scaleX, scaleY);

                //scaleTransform.ScaleX = scale;
                //scaleTransform.ScaleY = scale;

                //ViewModel.Transform(new(display.OuterBounds.Width, display.OuterBounds.Height - 48));
                ViewModel.ResetTransform();

                //scaleTransform.CenterX = 1000 / 2; // remember actual width of image;
                //scaleTransform.CenterY = 1000 / 2;
            }
            else if (this.Bounds.Width <= PART_Canvas.ActualWidth
                || this.Bounds.Height - 80 <= PART_Canvas.ActualHeight)
            {
                //var scaleX = this.Bounds.Width / 1000;
                //var scaleY = (this.Bounds.Height - 82) / 1000;

                //var scale = Math.Min(scaleX, scaleY);

                //scaleTransform.ScaleX = scale;
                //scaleTransform.ScaleY = scale;

                ViewModel.Transform(new(this.Bounds.Width, this.Bounds.Height - 80));

                //scaleTransform.CenterX = PART_Canvas.Width / 2;
                //scaleTransform.CenterY = PART_Canvas.Height / 2;
            }
            else if (PART_Border.ActualWidth <= PART_Canvas.ActualWidth
                || PART_Border.ActualHeight <= PART_Canvas.ActualHeight)
            {
                //var scaleX = PART_Border.ActualWidth / 1000;
                //var scaleY = PART_Border.ActualHeight / 1000;

                //var scale = Math.Min(scaleX, scaleY);

                //scaleTransform.ScaleX = scale;
                //scaleTransform.ScaleY = scale;

                ViewModel.Transform(new(PART_Border.ActualWidth, PART_Border.ActualHeight));

                //scaleTransform.CenterX = PART_Canvas.Width / 2;
                //scaleTransform.CenterY = PART_Canvas.Height / 2;
            }
            else
            {
                //scaleTransform.ScaleX = 1;
                //scaleTransform.ScaleY = 1;

                ViewModel.ResetTransform();

                //scaleTransform.CenterX = 500;
                //scaleTransform.CenterY = 500;
            }
        }
    }
}
