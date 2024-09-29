using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class SnipScreenWindow : Window
    {
        private bool isPointerReleased;
        public SnipScreenWindowViewModel ViewModel { get; }

        public SnipScreenWindow()
        {
            this.InitializeComponent();
            ViewModel = new();
            isPointerReleased = false;
        }

        public void PrepareWindow()
        {
            var monitors = Monitor.All.ToArray();
            var thisMonitor = Monitor.FromWindow(WinRT.Interop.WindowNative.GetWindowHandle(this));
            var otherMonitor = monitors.First(m => m.DeviceName != thisMonitor.DeviceName);
            var location = new System.Drawing.Point(otherMonitor.WorkingArea.X, otherMonitor.Bounds.Y);


            var bitmapImage = ScreenshotHelper.GetBitmapImageScreenshotForArea(
                location,
                System.Drawing.Point.Empty,
                new(otherMonitor.Bounds.Width, otherMonitor.Bounds.Height));

            ViewModel.SetWindowSize(new(otherMonitor.Bounds.Width, otherMonitor.Bounds.Height));
            ViewModel.SetBitmapImage(bitmapImage);
            ViewModel.SetResponceType(false);
            ViewModel.SetSelectedItem(SnipKinds.Recntangular);

            AppWindow.Move(new PointInt32(otherMonitor.WorkingArea.X, otherMonitor.WorkingArea.Y));
            var presenter = ((OverlappedPresenter)AppWindow.Presenter);
            presenter.Maximize();
            presenter.IsMinimizable = false;
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
            AppWindow.IsShownInSwitchers = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Exit();
            this.Close();
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            ViewModel.OnPointerPressed(e.GetPositionRelativeToCanvas((Canvas)sender));
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            ViewModel.OnPointerMoved(e.GetPositionRelativeToCanvas((Canvas)sender));
        }

        private async void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (!isPointerReleased)
            {
                isPointerReleased = true;
                await ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas((Canvas)sender));
                this.Close();
                isPointerReleased = false;
            }
        }
    }
}
