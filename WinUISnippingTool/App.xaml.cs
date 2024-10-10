using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System;
using WinUISnippingTool.Views;
using System.Linq;
using WinUISnippingTool.Models.MonitorInfo;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using WinUISnippingTool.Views.Pages;
using WinRT.WinUISnippingToolGenericHelpers;
using WinUISnippingTool.ViewModels;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        internal static MainWindow MainWindow { get; private set; }
        private MainWindowViewModel viewModel;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // To ensure all Notification handling happens in this process instance, register for
            // NotificationInvoked before calling Register(). Without this a new process will
            // be launched to handle the notification.
            AppNotificationManager notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += NotificationManager_NotificationInvoked;
            notificationManager.Register();

            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            var activationKind = activatedArgs.Kind;
            
            if (activationKind != ExtendedActivationKind.AppNotification)
            {
                LaunchAndBringToForegroundIfNeeded();
            }
            else
            {
                HandleNotification((AppNotificationActivatedEventArgs)activatedArgs.Data);
            }

            LaunchAndBringToForegroundIfNeeded();
        }

        private void NotificationManager_NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleNotification(args);
        }

        private void HandleNotification(AppNotificationActivatedEventArgs args)
        {
            MainWindow.DispatcherQueue.TryEnqueue(delegate
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

                                viewModel.AddImageFromSource(bmpImage, width, height);

                                ((OverlappedPresenter)MainWindow.AppWindow.Presenter).Restore();
                            }
                            break;
                    }
                }
                catch
                { }
            });
        }

        private void LaunchAndBringToForegroundIfNeeded()
        {
            if (MainWindow == null)
            {
                var monitors = Monitor.All.ToArray();
                viewModel = new();
                MainWindow = new();
                MainWindow.Closed += MainWindow_Closed;

                MainWindow.Prepare(viewModel, monitors);

                // Ensure the MainWindow is active
                MainWindow.Activate();

                MainWindow.NavigateToMainPage();

                // Additionally we show using our helper, since if activated via a app notification, it doesn't
                // activate the window correctly
                WindowHelper.ShowWindow(MainWindow);
            }
            else
            {
                MainWindow.Activate();
                WindowHelper.ShowWindow(MainWindow);
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            AppNotificationManager.Default.NotificationInvoked -= NotificationManager_NotificationInvoked;
            AppNotificationManager.Default.Unregister();
        }

        private static class WindowHelper
        {
            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetForegroundWindow(IntPtr hWnd);

            public static void ShowWindow(Window window)
            {
                // Bring the window to the foreground... first get the window handle...
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                // Restore window if minimized... requires DLL import above
                ShowWindow(hwnd, 0x00000009);

                // And call SetForegroundWindow... requires DLL import above
                SetForegroundWindow(hwnd);
            }
        }
    }
}
