using Microsoft.Graphics.DirectX;
using Microsoft.Graphics.Display;
using Microsoft.UI;
using Microsoft.UI.System;
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Capture;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using WinUISnippingTool.ViewModels;
using static System.Net.Mime.MediaTypeNames;

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

        private void Window_SizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
        {
            if (_contentLoaded)
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
            }
        }

        private byte[] buffer = new byte[8000];
        private GraphicsCaptureItem _item;
        private Direct3D11CaptureFramePool _framePool;

        private GraphicsCaptureSession _session;

        private void NewPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            Array.Clear(buffer);
            using (var bmpScreenshot = new Bitmap(1920, 1080))
            {
                using (var g = Graphics.FromImage(bmpScreenshot))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmpScreenshot.Size);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.DecodePixelWidth = 800;
                    using (MemoryStream stream = new MemoryStream(capacity: 8000))
                    {
                        bmpScreenshot.Save(stream, ImageFormat.Png);
                        stream.Position = 0;
                        bitmapImage.SetSource(stream.AsRandomAccessStream());
                    }

                    var image = new Microsoft.UI.Xaml.Controls.Image();
                    image.Stretch = Stretch.Uniform;
                    image.Source = bitmapImage;
                    image.Height = 1080;
                    image.Width = 1920;
                    PART_Canvas.Children.Add(image);
                }
            }
        }
    }
}
