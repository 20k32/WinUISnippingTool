using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinUISnippingTool.Models.MonitorInfo;

namespace WinUISnippingTool.Models.PageParameters;

public sealed class PageActivatedParameter
{
    public readonly DisplayArea CurrentDisplayArea;
    public readonly Monitor[] Monitors;
    public readonly SizeInt32 StartSize;
    public readonly nint WindowHandle;

    public PageActivatedParameter(
        DisplayArea currentDisplayArea,
        Monitor[] monitors,
        SizeInt32 startSize,
        nint windowHandle) => (StartSize, CurrentDisplayArea, Monitors, WindowHandle) =
                (startSize, currentDisplayArea, monitors, windowHandle);
}
