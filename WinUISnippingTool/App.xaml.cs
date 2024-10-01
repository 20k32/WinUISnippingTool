using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System;
using WinUISnippingTool.Views;
using WinUISnippingTool.Models;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; }

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
            LaunchAndBringToForegroundIfNeeded(args);
        }
            
        private void LaunchAndBringToForegroundIfNeeded(LaunchActivatedEventArgs args)
        {
            if (MainWindow == null)
            {
                var monitors = Monitor.All.ToArray();
                MainWindow = new MainWindow(monitors);
                //Frame rootFrame = new Frame();

                // rootFrame.NavigationFailed += RootFrame_NavigationFailed;
                // Navigate to the first page, configuring the new page
                // by passing required information as a navigation parameter
                //rootFrame.Navigate(typeof(MainPage), args.Arguments);

                // Place the frame in the current Window
                //m_window.Content = rootFrame;
                // Ensure the MainWindow is active
                MainWindow.Activate();

                // Additionally we show using our helper, since if activated via a app notification, it doesn't
                // activate the window correctly
                WindowHelper.ShowWindow(MainWindow);
            }
            else
            {
                MainWindow.Activate();
                //WindowHelper.ShowWindow(m_window);
            }
        }

        private void RootFrame_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
