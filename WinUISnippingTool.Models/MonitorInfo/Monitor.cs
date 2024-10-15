using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;

namespace WinUISnippingTool.Models.MonitorInfo;

public sealed class Monitor
{
    private Monitor(nint handle)
    {
        Handle = handle;
        var mi = new MONITORINFOEX();
        mi.cbSize = Marshal.SizeOf(mi);
        if (!GetMonitorInfo(handle, ref mi))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        DeviceName = mi.szDevice.ToString();
        Bounds = new RectInt32(mi.rcMonitor.left, mi.rcMonitor.top, mi.rcMonitor.right - mi.rcMonitor.left, mi.rcMonitor.bottom - mi.rcMonitor.top);
        WorkingArea = new RectInt32(mi.rcWork.left, mi.rcWork.top, mi.rcWork.right - mi.rcWork.left, mi.rcWork.bottom - mi.rcWork.top);
        IsPrimary = mi.dwFlags.HasFlag(MONITORINFOF.MONITORINFOF_PRIMARY);
    }

    public nint Handle { get; }
    public bool IsPrimary { get; }
    public RectInt32 WorkingArea { get; }
    public RectInt32 Bounds { get; }
    public string DeviceName { get; }

    public static IEnumerable<Monitor> All
    {
        get
        {
            var all = new List<Monitor>();
            EnumDisplayMonitors(nint.Zero, nint.Zero, (m, h, rc, p) =>
            {
                all.Add(new Monitor(m));
                return true;
            }, nint.Zero);
            return all;
        }
    }

    public override string ToString() => DeviceName;
    public static nint GetNearestFromWindow(nint hwnd) => MonitorFromWindow(hwnd, MFW.MONITOR_DEFAULTTONEAREST);
    public static nint GetDesktopMonitorHandle() => GetNearestFromWindow(GetDesktopWindow());
    public static nint GetShellMonitorHandle() => GetNearestFromWindow(GetShellWindow());
    public static Monitor FromWindow(nint hwnd, MFW flags = MFW.MONITOR_DEFAULTTONULL)
    {
        var h = MonitorFromWindow(hwnd, flags);
        return h != nint.Zero ? new Monitor(h) : null;
    }

    [Flags]
    public enum MFW
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002,
    }

    [Flags]
    public enum MONITORINFOF
    {
        MONITORINFOF_NONE = 0x00000000,
        MONITORINFOF_PRIMARY = 0x00000001,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public MONITORINFOF dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    private delegate bool MonitorEnumProc(nint monitor, nint hdc, nint lprcMonitor, nint lParam);

    [DllImport("user32")]
    private static extern nint GetDesktopWindow();

    [DllImport("user32")]
    private static extern nint GetShellWindow();

    [DllImport("user32")]
    private static extern bool EnumDisplayMonitors(nint hdc, nint lprcClip, MonitorEnumProc lpfnEnum, nint dwData);

    [DllImport("user32")]
    private static extern nint MonitorFromWindow(nint hwnd, MFW flags);

    [DllImport("user32", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(nint hmonitor, ref MONITORINFOEX info);
}