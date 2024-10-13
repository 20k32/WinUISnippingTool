using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinUISnippingTool.Models.MonitorInfo;
using WinUISnippingTool.ViewModels;

namespace WinUISnippingTool.Models.PageParameters;

internal sealed class MainPageActivatedParameter
{
    public readonly DisplayArea CurrentDisplayArea;
    public readonly Monitor[] Monitors;
    public readonly MainPageViewModel ViewModel;
    public readonly SizeInt32 StartSize;
    public readonly nint WindowHandle;

    public MainPageActivatedParameter(
        DisplayArea currentDisplayArea, 
        Monitor[] monitors, 
        MainPageViewModel viewModel, 
        SizeInt32 startSize,
        nint windowHandle)
        => (ViewModel, StartSize, CurrentDisplayArea, Monitors, WindowHandle) =
            (viewModel, startSize, currentDisplayArea, monitors, windowHandle);
}
