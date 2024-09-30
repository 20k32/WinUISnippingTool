using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
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
        private MonitorLocation currentWindowLocation; // because window is maximized

        public SnipScreenWindowViewModel ViewModel { get; private set; }

        public SnipScreenWindow()
        {
            this.InitializeComponent();
        }

        public void PrepareWindow(SnipScreenWindowViewModel viewModel, MonitorLocation location, SnipKinds snipKind)
        {
            currentWindowLocation = location;
            mainGrid.DataContext = viewModel;
            ViewModel = viewModel;
            ViewModel.SetCurrentMonitor(location.DeviceName);
            PART_Canvas.ItemsSource = ViewModel.GetOrAddCollectionForCurrentMonitor();

            var bitmapImage = ScreenshotHelper.GetBitmapImageScreenshotForArea(
                location.StartPoint,
                System.Drawing.Point.Empty,
                location.MonitorSize);

            ViewModel.AddImageSourceAndBrushFillForCurentMonitor(bitmapImage);
            ViewModel.AddShapeSourceForCurrentMonitor();
            ViewModel.SetWindowSize(location.MonitorSize);
            ViewModel.SetResponceType(false);
            ViewModel.SetSelectedItem(snipKind);

            if (!location.IsPrimary)
            {
                PART_TrayPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                var binding = new Binding();
                binding.Path = new(nameof(ViewModel.IsOverlayVisible));
                binding.Source = ViewModel;
                PART_TrayPanel.SetBinding(StackPanel.VisibilityProperty, binding);
            }

            AppWindow.Move(new PointInt32(location.StartPoint.X, location.StartPoint.Y));

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
            ViewModel.Exit(true);
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            if(currentWindowLocation.DeviceName != ViewModel.CurrentMonitorName)
            {
                ViewModel.SetCurrentMonitor(currentWindowLocation.DeviceName);
                
                ViewModel.SetImageSourceForCurrentMonitor();
                ViewModel.AddShapeSourceForCurrentMonitor();

                ViewModel.SetWindowSize(currentWindowLocation.MonitorSize);
            }
           

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

            await ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas((Canvas)sender));

            if(ViewModel.ResultFigure is not null)
            {
                ViewModel.Exit(false);
            }
            else
            {
                ViewModel.IsOverlayVisible = true;
            }
        }
    }
}
