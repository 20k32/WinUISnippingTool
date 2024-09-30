using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.WindowManagement;
using WinUISnippingTool.ViewModels;
using WinUISnippingTool.Models.Extensions;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage;
using WinRT.Interop;
using CommunityToolkit.WinUI.UI.Controls;
using WinUISnippingTool.Views.Pages;
using WinUISnippingTool.Models.PageParameters;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using System.Runtime.InteropServices;
using Microsoft.UI;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using WinUISnippingTool.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class MainWindow : Window
    {
        public MainWindow(Monitor[] monitors)
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            nint windowHandle = WindowNative.GetWindowHandle(this);
            FilePickerExtensions.SetWindowHandle(windowHandle);

            var appPresenter = (OverlappedPresenter)AppWindow.Presenter;
            var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);

            mainFrame.Navigate(typeof(MainPage),
                new MainPageParameter(appPresenter, displayArea, monitors));
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            Debug.WriteLine($"Window: {args.Size.Width} {args.Size.Height}");
        }
    }
}
