using Microsoft.UI.Xaml;
using Windows.Graphics;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Windows.UI.WindowManagement;
using System;
using WinUISnippingTool.Helpers;
using CommunityToolkit.WinUI;
using Windows.AI.MachineLearning.Preview;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Dispatching;
using WinUISnippingTool.Helpers.Saving;
using WinUISnippingTool.Models.VideoCapture;
using Windows.Storage;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.ViewModels.Resources;

public sealed partial class VideoCaptureWindow : Window
{
    private readonly VideoCaptureWindowViewModel viewModel;
    private bool exitRequested;
    public bool Exited { get; private set; }

    public VideoCaptureWindow(VideoCaptureWindowViewModel viewModel)
    {
        this.InitializeComponent();
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
        var sign = Math.Sign(viewModel.CurrentMonitor.StartPoint.X);
        sign = sign >= 0 ? 1 : sign;

        var actualWindth = viewModel.CurrentMonitor.Location.X == 0 
            ? viewModel.CurrentMonitor.Location.Width 
            : viewModel.CurrentMonitor.Location.X;

        var absX = Math.Abs(actualWindth);

        var coordX = sign * ((absX - mainPanel.ActualWidth) / 2);
        var coordY = 30;
        var newLocation = new RectInt32((int)coordX, coordY, (int)mainPanel.ActualWidth + 25, (int)mainPanel.ActualHeight + 20);

        AppWindow.MoveAndResize(newLocation);
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
