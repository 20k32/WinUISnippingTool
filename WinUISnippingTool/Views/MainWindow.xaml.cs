using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.WindowManagement;
using WinUISnippingTool.ViewModels;
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
using WinUISnippingTool.Models.MonitorInfo;
using Windows.Graphics;
using Windows.UI.WebUI;
using WinUISnippingTool.Helpers.Saving;
using WinUISnippingTool.Core;
using WinUISnippingTool.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class MainWindow : Window
{
    private nint windowHandle;
    private MainPageViewModel viewModel;
    private SizeInt32 startSize;
    private Monitor[] monitors;

    public MainWindow()
    {
        this.InitializeComponent();
        
        this.ExtendsContentIntoTitleBar = true;
        this.Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        viewModel.UnregisterHandlers();
    }

    public void Prepare(MainPageViewModel viewModel, Monitor[] monitors) 
    {
        windowHandle = WindowNative.GetWindowHandle(this);
        FilePickerExtensions.SetWindowHandle(windowHandle);

        this.viewModel = viewModel;
        this.monitors = monitors;

        WindowExtensions.SetMinSize(this, new(500, 500));
        startSize = new(800, 700);

        AppWindow.Resize(startSize);

        foreach(var monitor in monitors)
        {
            if (monitor.IsPrimary)
            {
                var posX = (monitor.Bounds.Width - startSize.Width) / 2;
                var posY = (monitor.Bounds.Height - startSize.Height) / 2;
                App.MainWindow.AppWindow.Move(new(posX, posY));
                break;
            }
        }
        
    }

    public void NavigateToMainPage()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        var parameter = new PageActivatedParameter(displayArea, monitors, startSize, windowHandle);

        mainFrame.Navigate(typeof(MainPage), parameter);
    }
}
