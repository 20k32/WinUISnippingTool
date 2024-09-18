using Microsoft.UI;
using Microsoft.UI.System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement;
using WinRT.Interop;
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
        }

        private void ThemeChanged(FrameworkElement sender, object args)
        {
            throw new NotImplementedException();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
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
                else if(!isScreenTinySized 
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
                    else if(isScreenTinySized
                        && args.Size.Width > 500)
                    {
                        PART_TakePictureButtonName.Visibility = Visibility.Visible;
                        isScreenTinySized = false;
                    }
                }
            }
        }
    }
}
