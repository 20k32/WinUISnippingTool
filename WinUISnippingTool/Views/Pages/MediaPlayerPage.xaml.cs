using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using WinUISnippingTool.Models.PageParameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class MediaPlayerPage : Page
{
    private Uri videoUri;
    public MediaPlayerPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(e.Parameter is MediaPlayerParameter parameter)
        {
            videoUri = parameter.Uri;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Mplayer.Source = MediaSource.CreateFromUri(videoUri);
        
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MainPage));
    }
}
