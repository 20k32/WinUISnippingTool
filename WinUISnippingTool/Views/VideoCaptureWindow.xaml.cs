using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinUISnippingTool.Models.VideoCapture;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Windows.Media.Devices;
using Windows.UI.WindowManagement;
using WinUISnippingTool.ViewModels;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using WinRT.Interop;
using WinUISnippingTool.Models.MonitorInfo;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views;

internal sealed partial class VideoCaptureWindow : Window
{
    private readonly VideoCaptureWindowViewModel viewModel;
    private readonly MonitorLocation currentMonitor;
    private bool exitRequested;
    public bool Exited { get; private set; }

    public VideoCaptureWindow(MonitorLocation currentMonitor, VideoCaptureWindowViewModel viewModel)
    {
        this.InitializeComponent();
        this.currentMonitor = currentMonitor;
        this.viewModel = viewModel;
        mainPanel.DataContext = viewModel;
        mainPanel.Loaded += MainPanel_Loaded;
        AppWindow.Closing += AppWindow_Closing;
        Exited = false;
        exitRequested = false;
    }

    private void ExitCore()
    {
        DispatcherQueue.TryEnqueue(() => 
        {
            Exited = true;
            viewModel.StopCapture();
            mainPanel.Loaded -= MainPanel_Loaded;
            AppWindow.Closing -= AppWindow_Closing;
        });
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (!exitRequested)
        {
            args.Cancel = true;
        }
        else
        {
            ExitCore();
        }
    }

    private void MainPanel_Loaded(object sender, RoutedEventArgs e)
    {
        var sign = Math.Sign(currentMonitor.StartPoint.X);
        sign = sign >= 0 ? 1 : sign;

        var actualWindth = currentMonitor.Location.X == 0 
            ? currentMonitor.Location.Width 
            : currentMonitor.Location.X;

        var absX = Math.Abs(actualWindth);

        var coordX = sign * ((absX - mainPanel.ActualWidth) / 2);
        var coordY = 30;
        var newLocation = new RectInt32((int)coordX, coordY, (int)mainPanel.ActualWidth + 25, (int)mainPanel.ActualHeight + 20);

        AppWindow.MoveAndResize(newLocation);
    }

    public async Task ActivateAndStartCaptureAsync()
    {
        this.Activate();
        await viewModel.StartCaptureAsync();
    }


    public void PrepareWindow()
    {
        AppWindow.IsShownInSwitchers = false;
        
        var presenter = ((OverlappedPresenter)AppWindow.Presenter);
        presenter.Minimize();
        presenter.IsMinimizable = false;
        presenter.IsMaximizable = false;
        presenter.IsResizable = false;
        presenter.SetBorderAndTitleBar(true, false);
        AppWindow.IsShownInSwitchers = true;
        presenter.IsAlwaysOnTop = true;
        this.ExtendsContentIntoTitleBar = true;
        SetTitleBar(PartBorder);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        exitRequested = true;
        ExitCore();
        DispatcherQueue.TryEnqueue(Close);
    }
}
