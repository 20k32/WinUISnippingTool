using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using WinRT;
using WinRT.Interop;
using WinUISnippingTool.Helpers;
using WinUISnippingTool.Helpers.Saving;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.Models.MonitorInfo;
using WinUISnippingTool.Models.PageParameters;
using WinUISnippingTool.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class MainPage : Page
{
    private nint windowHandle;

    private Canvas parentCanvas;
    private DisplayArea display;

    public MainPageViewModel ViewModel { get; private set; }

    public MainPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is PageActivatedParameter pageActivatedParamter)
        {
            windowHandle = pageActivatedParamter.WindowHandle;
            
            display = pageActivatedParamter.CurrentDisplayArea;

            ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            mainGrid.DataContext = ViewModel;

            foreach (var monitor in pageActivatedParamter.Monitors)
            {
                var monitorLocation = new MonitorLocation(monitor.Bounds, monitor.IsPrimary, monitor.DeviceName, monitor.Handle);
                ViewModel.AddMonitorLocation(monitorLocation);
            }

            ViewModel.SetWindowSize(new(display.OuterBounds.Width, display.OuterBounds.Height));
        }

        else if (e.Parameter is SettingsPageParameter settingsPageParameter)
        {
            ViewModel.TrySetAndLoadLocalizationWrapper(settingsPageParameter.BcpTag);
            ViewModel.SetSavingFolders(settingsPageParameter.SaveImageLocation, settingsPageParameter.SaveVideoLocation);
        }

        RegisterHandlers();
    }

    private void ViewModel_OnVideoModeExited(bool _)
    {
        WindowExtensions.ShowWindow(windowHandle);
    }

    private void ViewModel_OnVideoModeEntered()
    {
        WindowExtensions.HideWindow(windowHandle);
    }

    private void ViewModel_OnSnippingModeExited(bool byShortcut)
    {
        if (ViewModel.CanShowWindow)
        {
            WindowExtensions.ShowWindow(windowHandle);
        }
        else if (ViewModel.CanMinimizeWindow)
        {
            ((OverlappedPresenter)App.MainWindow.AppWindow.Presenter).Minimize();
        }
    }

    private void ViewModel_OnSnippingModeEntered()
    {
        WindowExtensions.HideWindow(windowHandle);
    }

    private void ThemeChanged(FrameworkElement sender, object args)
    {
        var titleBar = App.MainWindow.AppWindow.TitleBar;

        if (sender.ActualTheme == ElementTheme.Dark)
        {
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        }
        else
        {
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 1);
        }
    }

    private async Task<RenderTargetBitmap> RenderBmpCoreAsync()
    {
        var renderBitmap = new RenderTargetBitmap();
        await renderBitmap.RenderAsync(PART_Canvas, (int)ViewModel.CanvasWidth, (int)ViewModel.CanvasHeight);
        return renderBitmap;
    }

    private void PART_Canvas_Loaded(object sender, RoutedEventArgs e)
    {
        parentCanvas = (Canvas)PART_Canvas.ItemsPanelRoot;
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(Settings), new SettingsPageParameter(ViewModel.BcpTag, FolderExtensions.NewPicturesSavingFolder, FolderExtensions.NewVideosSavingFolder));
    }

    public void RegisterHandlers()
    {
        ViewModel.OnSnippingModeEntered += ViewModel_OnSnippingModeEntered;
        ViewModel.OnSnippingModeExited += ViewModel_OnSnippingModeExited;
        ViewModel.OnVideoModeEntered += ViewModel_OnVideoModeEntered;
        ViewModel.OnVideoModeExited += ViewModel_OnVideoModeExited;

        ViewModel.OnLargeSizeChanged += ViewModel_OnLargeSizeChanged;
        ViewModel.OnMiddleSizeChanged += ViewModel_OnMiddleSizeChanged;
        ViewModel.OnSmallSizeChanged += ViewModel_OnSmallSizeChanged;

        ViewModel.OnBitmapRequested += RenderBmpCoreAsync;
    }

    private void ViewModel_OnSmallSizeChanged(bool isSmall)
    {
        PART_TakePictureButtonName.Visibility = isSmall ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ViewModel_OnMiddleSizeChanged(bool isMiddle)
    {
        if (isMiddle)
        {
            PART_SubPane.Children.Remove(PART_RedactPicturePane);
            PART_MainPane.Children.Add(PART_RedactPicturePane);
            PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
        }
        else
        {
            PART_MainPane.Children.Remove(PART_RedactPicturePane);
            PART_SubPane.Children.Add(PART_RedactPicturePane);
            PART_TakePictureButtonName.Visibility = Visibility.Visible;
        }
    }

    private void ViewModel_OnLargeSizeChanged(bool isLarge)
    {
        PART_TakePictureButtonName.Visibility = isLarge ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnSnippingModeEntered -= ViewModel_OnSnippingModeEntered;
        ViewModel.OnSnippingModeExited -= ViewModel_OnSnippingModeExited;
        ViewModel.OnVideoModeEntered -= ViewModel_OnVideoModeEntered;
        ViewModel.OnVideoModeExited -= ViewModel_OnVideoModeExited;

        ViewModel.OnLargeSizeChanged -= ViewModel_OnLargeSizeChanged;
        ViewModel.OnMiddleSizeChanged -= ViewModel_OnMiddleSizeChanged;
        ViewModel.OnSmallSizeChanged -= ViewModel_OnSmallSizeChanged;

        ViewModel.OnBitmapRequested -= RenderBmpCoreAsync;
    }
}
