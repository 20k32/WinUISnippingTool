using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class SnipScreenWindow : Window
    {
        private bool isPointerReleased;
        public SnipScreenWindowViewModel ViewModel { get; }

        public SnipScreenWindow()
        {
            this.InitializeComponent();
            ViewModel = new();
            isPointerReleased = false;
        }

        public void PrepareWindow()
        {
            var presenter = ((OverlappedPresenter)AppWindow.Presenter);
            presenter.Maximize();
            presenter.IsMinimizable = false;
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
            AppWindow.IsShownInSwitchers = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Exit();
            this.Close();
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.OnPointerPressed(e.GetPositionRelativeToCanvas((Canvas)sender));
            e.Handled = true;
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.OnPointerMoved(e.GetPositionRelativeToCanvas((Canvas)sender));
            e.Handled = true;
        }

        private async void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isPointerReleased)
            {
                isPointerReleased = true;
                await ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas((Canvas)sender));
                this.Close();
                isPointerReleased = false;
            }
        }
    }
}
