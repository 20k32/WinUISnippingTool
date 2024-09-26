using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using System.Runtime.InteropServices;
using System;
using WinUISnippingTool.Views;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using WinUISnippingTool.Views.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;
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
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            //m_window = new MainWindow();

            AppNotificationManager notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += NotificationManager_NotificationInvoked;
            notificationManager.Register();

            var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            var activationKind = activatedArgs.Kind;
            if (activationKind != ExtendedActivationKind.AppNotification)
            {
                LaunchAndBringToForegroundIfNeeded(args);
            }
            else
            {
                HandleNotification((AppNotificationActivatedEventArgs)activatedArgs.Data);
            }


            //m_window.Activate();
        }

        private void LaunchAndBringToForegroundIfNeeded(LaunchActivatedEventArgs args)
        {
            if (m_window == null)
            {
                m_window = new MainWindow();
                //Frame rootFrame = new Frame();

               // rootFrame.NavigationFailed += RootFrame_NavigationFailed;
                // Navigate to the first page, configuring the new page
                // by passing required information as a navigation parameter
                //rootFrame.Navigate(typeof(MainPage), args.Arguments);

                // Place the frame in the current Window
                //m_window.Content = rootFrame;
                // Ensure the MainWindow is active
                m_window.Activate();

                // Additionally we show using our helper, since if activated via a app notification, it doesn't
                // activate the window correctly
                WindowHelper.ShowWindow(m_window);
            }
            else
            {
                m_window.Activate();
                //WindowHelper.ShowWindow(m_window);
            }
        }

        private void RootFrame_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void NotificationManager_NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            HandleNotification(args);
        }

        private void HandleNotification(AppNotificationActivatedEventArgs args)
        {
            // Use the dispatcher from the window if present, otherwise the app dispatcher
            var dispatcherQueue = m_window?.DispatcherQueue ?? DispatcherQueue.GetForCurrentThread();


            dispatcherQueue.TryEnqueue(async delegate
            {

                try
                {
                    switch (args.Arguments["snapshotStatus"])
                    {
                        case "snapshotTaken":
                            {
                                string uriStr = args.Arguments["snapshotUri"];
                                var uri = new Uri(uriStr);
                                var image = new Image()
                                {
                                    Source = new BitmapImage
                                    {
                                        UriSource = uri,
                                    }
                                };
                                var width = int.Parse(args.Arguments["snapshotWidth"]);
                                var height = int.Parse(args.Arguments["snapshotHeight"]);

                                //todo: launch vm methods
                                /*((MainWindow)m_window).ViewModel.AddImage(image, width, height);
                                ((MainWindow)m_window).TransformImage();
                                m_window.Activate();*/

                                m_window.Activate();

                            }
                            break;
                    }
                }
                catch
                { }
            });
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
