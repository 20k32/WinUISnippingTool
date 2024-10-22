using CommunityToolkit.Mvvm.Messaging;
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.Imaging;
using WinUISnippingTool.Helpers;
using WinUISnippingTool.Helpers.DirectX;
using WinUISnippingTool.Helpers.Saving;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Models.MonitorInfo;
using WinUISnippingTool.Models.VideoCapture;
using WinUISnippingTool.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.ViewModels.Resources;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SnipScreenWindow : Window
{
    public bool IsClosed { get; private set; }

    public SnipScreenWindowViewModel ViewModel { get; private set; }

    public SnipScreenWindow(SnipScreenWindowViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        PartGrid.DataContext = viewModel;
    }


    public void PrepareWindow(MonitorLocation location)
    {
        PartCanvas.ItemsSource = ViewModel.GetOrAddCollectionForCurrentMonitor();

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


        var rectInt32 = new RectInt32()
        {
            X = location.StartPoint.X,
            Y = location.StartPoint.Y,
            Height = (int)location.MonitorSize.Height,
            Width = (int)location.MonitorSize.Width
        };
        this.ExtendsContentIntoTitleBar = true;
        AppWindow.MoveAndResize(rectInt32);
        SetupPresenter();
    }

    /// <summary>
    /// <code>OverlappedPresenter.SetBorderAndTitleBar(true, ...)</code>
    /// causes binding-bugs and access-violation bug on closing this window from any thread
    /// not only on closing but on any async operation (any inner async operation) related
    /// to window (fields including vm)
    /// </summary>
    private void SetupPresenter()
    {
        var presenter = ((OverlappedPresenter)AppWindow.Presenter);

        presenter.IsMinimizable = false;
        presenter.IsMaximizable = false;
        presenter.IsResizable = false;

#if DEBUG
        presenter.IsAlwaysOnTop = false;
#else
        presenter.IsAlwaysOnTop = true;
#endif
        AppWindow.IsShownInSwitchers = false;
        presenter.SetBorderAndTitleBar(false, false);
    }
}
