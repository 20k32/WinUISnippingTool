using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
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
        public SnipScreenWindowViewModel ViewModel { get; }

        public SnipScreenWindow(BitmapImage bmpImage, SnipKinds kind)
        {
            this.InitializeComponent();
            ViewModel = new(bmpImage, kind);
            var presenter = ((OverlappedPresenter)AppWindow.Presenter);
            presenter.Maximize();
            //presenter.IsMinimizable = false;
            //presenter.IsMaximizable = false;
            //presenter.IsResizable = false;
           // AppWindow.IsShownInSwitchers = false;
            //presenter.SetBorderAndTitleBar(false, false);
        }


        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Exit();
            this.Close();
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.OnPointerPressed(e.GetPositionRelativeToCanvas((Canvas)sender));
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.OnPointerMoved(e.GetPositionRelativeToCanvas((Canvas)sender));
        }

        private async void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await ViewModel.OnPointerReleased(e.GetPositionRelativeToCanvas((Canvas)sender));
            this.Close();
        }
    }
}
