using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinUISnippingTool.Models.MonitorInfo;
using WinUISnippingTool.ViewModels;

namespace WinUISnippingTool.Models.PageParameters;

internal sealed class MainPageActivatedParameter
{
    public readonly OverlappedPresenter AppWindowPresenter;
    public readonly DisplayArea CurrentDisplayArea;
    public readonly Monitor[] Monitors;
    public readonly MainWindowViewModel ViewModel;
    public readonly SizeInt32 StartSize;

    public MainPageActivatedParameter(OverlappedPresenter appPresenter, 
        DisplayArea currentDisplayArea, 
        Monitor[] monitors, 
        MainWindowViewModel viewModel, SizeInt32 startSize)
        => (ViewModel, StartSize, AppWindowPresenter, CurrentDisplayArea, Monitors) = (viewModel, startSize, appPresenter, currentDisplayArea, monitors);
}
