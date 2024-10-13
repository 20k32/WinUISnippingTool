using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Widgets.Feeds.Providers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Models.MonitorInfo;
using WinUISnippingTool.Models.VideoCapture;
using WinUISnippingTool.ViewModels;
using WinUISnippingTool.Views.UserControls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class SnipScreenWindow : Window
{
    private MonitorLocation currentWindowLocation;

    public SnipScreenWindowViewModel ViewModel { get; private set; }

    public SnipScreenWindow()
    {
        this.InitializeComponent();
    }

    public async Task PrepareWindowAsync(SnipScreenWindowViewModel viewModel, MonitorLocation location, SnipShapeKind snipKind, bool byShortcut)
    {
        currentWindowLocation = location;
        PartGrid.DataContext = viewModel;
        ViewModel = viewModel;
        ViewModel.ResetModel();
        ViewModel.SetCurrentMonitor(location.DeviceName);
        PartCanvas.ItemsSource = ViewModel.GetOrAddCollectionForCurrentMonitor();

        Debug.WriteLine($"Begin screenshot");

        var softwareBitmap = await ScreenshotExtensions.GetSoftwareBitmapImageScreenshotForAreaAsync(
            location.StartPoint,
            System.Drawing.Point.Empty,
            location.MonitorSize);

        Debug.WriteLine($"End screenshot");

        var softwareBitmapSource = new SoftwareBitmapSource();
        await softwareBitmapSource.SetBitmapAsync(softwareBitmap);

        PrepareWindow(location, softwareBitmapSource, softwareBitmap, byShortcut, snipKind);

        if (!location.IsPrimary)
        {
            PartBorder.Visibility = Visibility.Collapsed;
        }
        else
        {
            ViewModel.SetPrimaryMonitor(location);

            var binding = new Binding();
            binding.Path = new(nameof(ViewModel.IsOverlayVisible));
            binding.Source = ViewModel;
            PartBorder.SetBinding(Border.VisibilityProperty, binding);
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

    private void PrepareWindow(
        MonitorLocation location,
        SoftwareBitmapSource softwareBitmapSource,
        SoftwareBitmap softwareBitmap,
        bool byShortcut,
        SnipShapeKind snipKind)
    {
        ViewModel.AddImageSourceAndBrushFillForCurentMonitor(softwareBitmapSource);
        ViewModel.AddSoftwareBitmapForCurrentMonitor(location, softwareBitmap);
        ViewModel.AddShapeSourceForCurrentMonitor();
        ViewModel.SetWindowSize(location.MonitorSize);
        ViewModel.SetResponceType(byShortcut);
        ViewModel.SetImageSourceForCurrentMonitor();
        ViewModel.SetSelectedItem(snipKind);
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Exit();
    }

    private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;

        if (currentWindowLocation.DeviceName != ViewModel.CurrentMonitorName)
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
        ViewModel.TryExit();
    }
}
