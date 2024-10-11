using System;
using System.Collections.Generic;
using Windows.Foundation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace WinUISnippingTool.Models;

internal readonly struct MonitorLocation
{
    public readonly HMONITOR HandleMonitor;
    public readonly string DeviceName;
    public readonly RectInt32 Location;
    public readonly bool IsPrimary;
    public readonly Size MonitorSize => new(Location.Width, Location.Height);
    public readonly System.Drawing.Point StartPoint => new(Location.X, Location.Y);

    public MonitorLocation(RectInt32 location, bool isPrimary, string deviceName, nint handleMonitor)
        => (Location, IsPrimary, DeviceName, HandleMonitor) = (location, isPrimary, deviceName, (HMONITOR)handleMonitor);
}
