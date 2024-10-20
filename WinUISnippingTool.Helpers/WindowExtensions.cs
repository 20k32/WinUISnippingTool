using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;

namespace WinUISnippingTool.Helpers;

public static class WindowExtensions
{
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PointInfo
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [Flags]
    private enum WindowLongIndexFlags : int
    {
        WndProc = -4,
    }

    private enum WindowMessage : int
    {
        GetMinMaxInfo = 0x0024,
    }

    private enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }

    private const int SwHide = 0;
    private const int SwShow = 5;
    private static SizeInt32 minSize;
    private static WinProc newWndProc = null;
    private static IntPtr oldWndProc = IntPtr.Zero;
    private delegate IntPtr WinProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);

    public static bool ShowWindow(nint windowHandle) => ShowWindow(windowHandle, SwShow);
    public static bool HideWindow(nint windowHandle) => ShowWindow(windowHandle, SwHide);

    public static Size CalculateDesiredSizeForMonitor(MonitorLocation location)
    {
        var dpiTuple = WindowExtensions.GetDpiForMonitor(location.HandleMonitor);

        var scaleX = dpiTuple.dpiX / (double)CoreConstants.DefaultMonitorDpi;
        var scaleY = dpiTuple.dpiY / (double)CoreConstants.DefaultMonitorDpi;

        var newSize = new Size((int)(location.MonitorSize.Width / scaleX), (int)(location.MonitorSize.Height / scaleY));

        return newSize;
    }

    public static (uint dpiX, uint dpiY) GetDpiForMonitor(nint handleMonitor)
    {
        GetDpiForMonitor(handleMonitor, DpiType.Effective, out var dpiX, out var dpiY);

        return (dpiX, dpiY);
    }

    //todo: libraryimport instead of dllimport
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("Shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

    [DllImport("user32.dll")]
    public static extern int GetDpiForWindow(IntPtr hwnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);

    public static void SetMinSize(Window window, SizeInt32 minSize)
    {
        var hwnd = GetWindowHandleForCurrentWindow(window);
        WindowExtensions.minSize = minSize;

        newWndProc = new WinProc(WndProc);
        oldWndProc = SetWindowLongPtr(hwnd, WindowLongIndexFlags.WndProc, newWndProc);
    }

    private static IntPtr GetWindowHandleForCurrentWindow(object target) =>
        WinRT.Interop.WindowNative.GetWindowHandle(target);

    private static IntPtr WndProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam)
    {
        if(Msg == WindowMessage.GetMinMaxInfo)
        {
            var dpi = GetDpiForWindow(hWnd);
            var scalingFactor = (float)dpi / 96;

            var minPointWrapper = Marshal.PtrToStructure<PointInfo>(lParam);
            minPointWrapper.ptMinTrackSize.X = (int)(minSize.Width * scalingFactor);
            minPointWrapper.ptMinTrackSize.Y = (int)(minSize.Height * scalingFactor);

            Marshal.StructureToPtr(minPointWrapper, lParam, true);
        }

        return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, newProc);
        else
            return new IntPtr(SetWindowLong32(hWnd, nIndex, newProc));
    }
}
