using Microsoft.UI.Windowing;
using WinUISnippingTool.Models.MonitorInfo;

namespace WinUISnippingTool.Models.PageParameters;

internal sealed class MainPageParameter
{
    public readonly OverlappedPresenter AppWindowPresenter;
    public readonly DisplayArea CurrentDisplayArea;
    public readonly Monitor[] Monitors;


    public MainPageParameter(OverlappedPresenter appPresenter, DisplayArea currentDisplayArea, Monitor[] monitors)
        => (AppWindowPresenter, CurrentDisplayArea, Monitors) = (appPresenter, currentDisplayArea, monitors);
}
