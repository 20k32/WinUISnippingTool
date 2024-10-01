using Microsoft.Graphics.Canvas;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
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
    private DisplayArea display;
    private bool isScreenTinySized;
    private bool isScreenSmallSized;
    private bool isScreenMiddleSized;
    private Canvas parentCanvas;
    private double PageWidth;
    private double PageHeight;
    private DispatcherTimer timer;
    private OverlappedPresenter appWindowPersenter;
    
    public bool SizeChanginRequested;

    public MainWindowViewModel ViewModel { get; private set; }

    public MainPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
        AppNotificationManager.Default.NotificationInvoked += NotificationManager_NotificationInvoked;
        AppNotificationManager.Default.Register();
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.Closed -= MainWindow_Closed;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        ViewModel.UnregisterHandlers();
        AppNotificationManager.Default.NotificationInvoked -= NotificationManager_NotificationInvoked;
        AppNotificationManager.Default.Unregister();
    }

    private void NotificationManager_NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        HandleNotification(args);
    }

    private void HandleNotification(AppNotificationActivatedEventArgs args)
    {
        DispatcherQueue.TryEnqueue(delegate
        {
            try
            {
                switch (args.Arguments["snapshotStatus"])
                {
                    case "snapshotTaken":
                        {
                            string uriStr = args.Arguments["snapshotUri"];
                            var uri = new Uri(uriStr);
                            var bmpImage = new BitmapImage
                            {
                                UriSource = uri,
                            };

                            var width = int.Parse(args.Arguments["snapshotWidth"]);
                            var height = int.Parse(args.Arguments["snapshotHeight"]);

                            ViewModel.AddImageFromSource(bmpImage, width, height);
                            appWindowPersenter.Restore();
                        }
                        break;
                }
            }
            catch
            { }
        });
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is MainPageParameter mainPageParameter)
        {
            contentLoaded = true;
            
            ViewModel = new();
            ViewModel.OnNewImageAdded += PART_Canvas_SizeChanged;

            appWindowPersenter = mainPageParameter.AppWindowPresenter;
            display = mainPageParameter.CurrentDisplayArea;

            mainGrid.DataContext = ViewModel;


            foreach(var monitor in mainPageParameter.Monitors)
            {
                var monitorLocation = new MonitorLocation(monitor.Bounds, monitor.IsPrimary, monitor.DeviceName);
                ViewModel.AddMonitorLocation(monitorLocation);
            }

            ViewModel.SetWindowSize(new(display.OuterBounds.Width, display.OuterBounds.Height));

            timer = new()
            {
                Interval = System.TimeSpan.FromMilliseconds(500),
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        if (e.Parameter is SettingsPageParameter settingsPageParameter)
        {
            ViewModel.TrySetAndLoadLocalizationWrapper(settingsPageParameter.BcpTag);
            PicturesFolderExtensions.NewSavingFolder = settingsPageParameter.SaveImageLocation;
        }
    }

    private void Timer_Tick(object sender, object e)
    {
        if (SizeChanginRequested)
        {
            PART_Canvas_SizeChanged();
            SizeChanginRequested = false;
        }
    }

    private void ThemeChanged(FrameworkElement sender, object args)
    {
        //throw new NotImplementedException();
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        appWindowPersenter.Minimize(false);
        ViewModel.EnterSnippingMode(false);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        if (contentLoaded)
        {
            PageWidth = args.NewSize.Width;
            PageHeight = args.NewSize.Height;

            if (isScreenSmallSized)
            {
                PageHeight -= 32;
            }

            if (!isScreenMiddleSized
                 && args.NewSize.Width < CoreConstants.MinLargeWidth)
            {
                PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                isScreenMiddleSized = true;
            }
            else if (!isScreenSmallSized
                     && args.NewSize.Width < CoreConstants.MinMediumWidht)
            {
                PART_MainPane.Children.Remove(PART_RedactPicturePane);
                PART_SubPane.Children.Add(PART_RedactPicturePane);
                PART_TakePictureButtonName.Visibility = Visibility.Visible;
                isScreenSmallSized = true;
            }
            else if (!isScreenTinySized
                    && args.NewSize.Width < CoreConstants.MinSmallWidth)
            {
                PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                isScreenTinySized = true;
            }
            else
            {
                if (isScreenMiddleSized
                         && args.NewSize.Width > CoreConstants.MinLargeWidth)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenMiddleSized = false;
                }
                else if (isScreenSmallSized
                    && args.NewSize.Width > CoreConstants.MinMediumWidht)
                {
                    PART_SubPane.Children.Remove(PART_RedactPicturePane);
                    PART_MainPane.Children.Add(PART_RedactPicturePane);
                    PART_TakePictureButtonName.Visibility = Visibility.Collapsed;
                    isScreenSmallSized = false;
                }
                else if (isScreenTinySized
                    && args.NewSize.Width > CoreConstants.MinSmallWidth)
                {
                    PART_TakePictureButtonName.Visibility = Visibility.Visible;
                    isScreenTinySized = false;
                }
            }
            Debug.WriteLine($"Page: {args.NewSize.Width} {args.NewSize.Height}");
            SizeChanginRequested = true;
        }
    }

    public void TransformImage()
    {
        if (PART_Border.ActualWidth <= PART_Canvas.Width
                   || PART_Border.ActualHeight <= PART_Canvas.Height)
        {
            ViewModel.Transform(new(PART_Border.ActualWidth, PART_Border.ActualHeight));
        }
    }

    private void PART_Canvas_SizeChanged()
    {
        if (PageWidth + 32 <= ViewModel.CanvasWidth
            || PageHeight - 64 <= ViewModel.CanvasHeight)
        {
            ViewModel.Transform(new(PageWidth - 32, PageHeight - 64));
        }
        else
        {
            ViewModel.ResetTransform();
        }
    }

    private void EnterSnippingModeByShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        appWindowPersenter.Minimize(false);
        ViewModel.EnterSnippingMode(true);
        args.Handled = true;
    }

    private void GlobalUndoShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.GlobalUndoCommand?.Execute(null);
    }

    private void GlobalRedoShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.GlobalRedoCommand?.Execute(null);
    }

    private async Task<RenderTargetBitmap> SaveBmpCoreAsync()
    {
        var renderBitmap = new RenderTargetBitmap();
        await renderBitmap.RenderAsync(PART_Canvas, (int)ViewModel.CanvasWidth, (int)ViewModel.CanvasHeight);
        return renderBitmap;
    }

    private async void SaveToClipboardShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        var renderBitmap = await SaveBmpCoreAsync();
        await ViewModel.SaveBmpToClipboardAsync(renderBitmap);
    }

    private async void SaveFileDialogShortcut(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        var renderBitmap = await SaveBmpCoreAsync();
        await ViewModel.SaveBmpToFileAsync(renderBitmap);
    }

    private void Canvas_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ViewModel.OnPointerPressed(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void Canvas_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ViewModel.OnPointerMoved(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void Canvas_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas(parentCanvas));
    }

    private void PART_Canvas_Loaded(object sender, RoutedEventArgs e)
    {
        parentCanvas = (Canvas)PART_Canvas.ItemsPanelRoot;
    }

    private async void CropButton_Click(object sender, RoutedEventArgs e)
    {
        var renderBitmap = await SaveBmpCoreAsync();
        await ViewModel.EnterCroppingMode(renderBitmap);
    }

    private void DecropButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ExitCroppingMode();
    }

    private void CommitCropButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CommitCrop();
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(Settings), new SettingsPageParameter()
        {
            BcpTag = ViewModel.BcpTag,
            SaveImageLocation = PicturesFolderExtensions.NewSavingFolder
        });
    }

    private async void SaveBmpToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var renderBitmap = await SaveBmpCoreAsync();
        await ViewModel.SaveBmpToClipboardAsync(renderBitmap);
    }

    private async void SaveBmpToFile_Click(object sender, RoutedEventArgs e)
    {
        var renderBitmap = await SaveBmpCoreAsync();
        await ViewModel.SaveBmpToFileAsync(renderBitmap);
    }
}
