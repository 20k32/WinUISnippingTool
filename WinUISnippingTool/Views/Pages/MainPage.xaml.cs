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
    private static bool contentLoaded;
    
    private nint windowHandle;
    
    private bool isScreenSmallSized;
    private bool isScreenMediumSized;
    private bool isScreenLargeSized;

    private bool scaleRequested;

    private double pageWidth;
    private double pageHeight;
    
    private Canvas parentCanvas;
    private DispatcherTimer timer;
    private DisplayArea display;
    
    public bool SizeChangingRequested;

    public MainPageViewModel ViewModel { get; private set; }

    public MainPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(e.Parameter is PageActivatedParameter pageActivatedParamter)
        {
            windowHandle = pageActivatedParamter.WindowHandle;
            pageWidth = pageActivatedParamter.StartSize.Width;
            pageHeight = pageActivatedParamter.StartSize.Height;

            display = pageActivatedParamter.CurrentDisplayArea;

            ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            mainGrid.DataContext = ViewModel;

            foreach (var monitor in pageActivatedParamter.Monitors)
            {
                var monitorLocation = new MonitorLocation(monitor.Bounds, monitor.IsPrimary, monitor.DeviceName, monitor.Handle);
                ViewModel.AddMonitorLocation(monitorLocation);
            }

            ViewModel.SetWindowSize(new(display.OuterBounds.Width, display.OuterBounds.Height));

            timer = new()
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };

            timer.Tick += Timer_Tick;
            timer.Start();

            contentLoaded = true;
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

    private void Timer_Tick(object sender, object e)
    {
        if (SizeChangingRequested)
        {
            PART_Canvas_SizeChanged();
            SizeChangingRequested = false;
        }
    }

    private void ThemeChanged(FrameworkElement sender, object args)
    {
        var titleBar = App.MainWindow.AppWindow.TitleBar;

        if(sender.ActualTheme == ElementTheme.Dark)
        {
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        }
        else
        {
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 1);
        }
    }

    private async void SnippingModeButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.EnterSnippingModeAsync(false);
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        if (contentLoaded)
        {
            pageWidth = args.NewSize.Width;
            pageHeight = args.NewSize.Height;

            if (isScreenMediumSized)
            {
                pageHeight -= CoreConstants.BottomPanelHeight;
            }

            if (!isScreenLargeSized
                 && args.NewSize.Width < CoreConstants.MinLargeWidth)
            {
                PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                isScreenLargeSized = true;
            }
            else if (!isScreenMediumSized
                     && args.NewSize.Width < CoreConstants.MinMediumWidth)
            {
                PART_MainPane.Children.Remove(PART_RedactPicturePane);
                PART_SubPane.Children.Add(PART_RedactPicturePane);
                PART_TakePictureButtonName.Visibility = Visibility.Visible;
                isScreenMediumSized = true;
            }
            else if (!isScreenSmallSized
                    && args.NewSize.Width < CoreConstants.MinSmallWidth)
            {
                PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                isScreenSmallSized = true;
            }
            else
            {
                if (isScreenLargeSized
                         && args.NewSize.Width > CoreConstants.MinLargeWidth)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenLargeSized = false;
                }
                else if (isScreenMediumSized
                    && args.NewSize.Width > CoreConstants.MinMediumWidth)
                {
                    PART_SubPane.Children.Remove(PART_RedactPicturePane);
                    PART_MainPane.Children.Add(PART_RedactPicturePane);
                    PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                    isScreenMediumSized = false;
                }
                else if (isScreenSmallSized
                    && args.NewSize.Width > CoreConstants.MinSmallWidth)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenSmallSized = false;
                }
            }
            Debug.WriteLine($"Page: {args.NewSize.Width} {args.NewSize.Height}");
            SizeChangingRequested = true;
        }
    }

    private void PART_Canvas_SizeChanged()
    {
        if (pageWidth + CoreConstants.MarginLeftRight <= ViewModel.CanvasWidth
            || pageHeight - CoreConstants.MarginTopBottom <= ViewModel.CanvasHeight)
        {
            var currentSize = new Size(pageWidth - CoreConstants.MarginLeftRight, pageHeight - CoreConstants.MarginTopBottom);
            ViewModel.Transform(currentSize);

            if (scaleRequested)
            {
                ViewModel.ResetScaleValues();
                scaleRequested = false;
            }
        }
        else
        {
            ViewModel.ResetTransform();
        }
    }

    private async void EnterSnippingModeByShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        await ViewModel.EnterSnippingModeAsync(true);
    }

    private void GlobalUndoShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (ViewModel.GlobalUndoCommand.CanExecute(null))
        {
            ViewModel.GlobalUndoCommand.Execute(null);
        }
    }

    private void GlobalRedoShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.GlobalRedoCommand.CanExecute(null))
        {
            ViewModel.GlobalRedoCommand.Execute(null);
        }
    }

    private async Task<RenderTargetBitmap> RenderBmpCoreAsync()
    {
        var renderBitmap = new RenderTargetBitmap();
        await renderBitmap.RenderAsync(PART_Canvas, (int)ViewModel.CanvasWidth, (int)ViewModel.CanvasHeight);
        return renderBitmap;
    }

    private async void SaveToClipboardShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        var isImageType = ViewModel.IsDrawingElementTypeOfImage();

        if(isImageType is not null 
           && isImageType.Value is true)
        {
            var renderBitmap = await RenderBmpCoreAsync();
            await ViewModel.SaveBmpToClipboardAsync(renderBitmap);
        }
    }

    private async Task SaveCoreAsync()
    {
        var isImageType = ViewModel.IsDrawingElementTypeOfImage();

        if(isImageType is not null)
        {
            if (isImageType.Value is true)
            {
                var renderBitmap = await RenderBmpCoreAsync();
                await ViewModel.SaveBitmapAsync(renderBitmap);
            }
            else if (isImageType.Value is false)
            {
                await ViewModel.SaveVideoAsync();
            }
        }
    }

    private async void SaveFileDialogShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await SaveCoreAsync();
    }

    private void Canvas_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;

        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsMiddleButtonPressed && scaleRequested)
        {
            var currentSize = new Size(pageWidth - CoreConstants.MarginLeftRight, pageHeight - CoreConstants.MarginTopBottom);
            ViewModel.Transform(currentSize);
            ViewModel.ResetScaleValues();

            scaleRequested = false;
        }
        else if(!point.Properties.IsMiddleButtonPressed)
        {
            ViewModel.OnPointerPressed(e.GetPositionRelativeToCanvas(parentCanvas));
        }
    }

    private void Canvas_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;

        var point = e.GetCurrentPoint(this);

        ViewModel.OnPointerMoved(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void Canvas_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;

        var point = e.GetCurrentPoint(this);

        ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void PART_Canvas_Loaded(object sender, RoutedEventArgs e)
    {
        parentCanvas = (Canvas)PART_Canvas.ItemsPanelRoot;
    }

    private async void CropButton_Click(object sender, RoutedEventArgs e)
    {
        var renderBitmap = await RenderBmpCoreAsync();
        await ViewModel.EnterCroppingMode(renderBitmap);
    }

    private void DecropButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ExitCroppingMode(true);
    }

    private void CommitCropButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CommitCrop();
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(Settings), new SettingsPageParameter(ViewModel.BcpTag, FolderExtensions.NewPicturesSavingFolder, FolderExtensions.NewVideosSavingFolder));
    }

    private async void SaveBmpToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var isImageType = ViewModel.IsDrawingElementTypeOfImage();

        if(isImageType.Value is true)
        {
            var renderBitmap = await RenderBmpCoreAsync();
            await ViewModel.SaveBmpToClipboardAsync(renderBitmap);
        }
    }

    private async void SaveBmpToFile_Click(object sender, RoutedEventArgs e)
    {
        var renderBitmap = await RenderBmpCoreAsync();
        await SaveCoreAsync();
    }

    public void RegisterHandlers()
    {
        ViewModel.OnNewImageAdded += PART_Canvas_SizeChanged;
        ViewModel.OnSnippingModeEntered += ViewModel_OnSnippingModeEntered;
        ViewModel.OnSnippingModeExited += ViewModel_OnSnippingModeExited;
        ViewModel.OnVideoModeEntered += ViewModel_OnVideoModeEntered;
        ViewModel.OnVideoModeExited += ViewModel_OnVideoModeExited;
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnNewImageAdded -= PART_Canvas_SizeChanged;
        ViewModel.OnSnippingModeEntered -= ViewModel_OnSnippingModeEntered;
        ViewModel.OnSnippingModeExited -= ViewModel_OnSnippingModeExited;
        ViewModel.OnVideoModeEntered -= ViewModel_OnVideoModeEntered;
        ViewModel.OnVideoModeExited -= ViewModel_OnVideoModeExited;
    }

    private void Page_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;

        if(parentCanvas is not null)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;

            ViewModel.ScaleCanvas(delta);

            scaleRequested = true;
        }
    }
}
