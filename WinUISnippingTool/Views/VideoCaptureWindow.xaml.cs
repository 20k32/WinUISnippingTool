using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.VideoCapture;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Windows.Media.Devices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views;

internal sealed partial class VideoCaptureWindow : Window
{
    private readonly MonitorLocation currentMonitor;
    private readonly VideoCaptureHelper captureHelper;

    public VideoCaptureWindow(MonitorLocation currentMonitor, VideoCaptureHelper captureHelper)
    {
        this.InitializeComponent();
        this.currentMonitor = currentMonitor;
        this.captureHelper = captureHelper;

        mainPanel.Loaded += MainPanel_Loaded;
    }

    private void MainPanel_Loaded(object sender, RoutedEventArgs e)
    {
        var coordX = ((currentMonitor.MonitorSize.Width - mainPanel.ActualWidth) / 2) - mainPanel.ActualWidth / 2;
        var coordY = 30;
        var newLocation = new RectInt32((int)coordX, coordY, (int)mainPanel.ActualWidth + 10, (int)mainPanel.ActualHeight + 10);

        var presenter = ((OverlappedPresenter)AppWindow.Presenter);
        presenter.Restore();
        AppWindow.MoveAndResize(newLocation);
    }

    public void PrepareWindow(SizeInt32 windowSize)
    {
         var presenter = ((OverlappedPresenter)AppWindow.Presenter);
         presenter.Minimize();
        
         presenter.IsMinimizable = true;
         presenter.IsMaximizable = false;
         presenter.IsResizable = false;
         AppWindow.IsShownInSwitchers = true;
         presenter.SetBorderAndTitleBar(false, false);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        captureHelper.StopScreenCapture();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        captureHelper.StopScreenCapture();
        this.Close();
    }
}
