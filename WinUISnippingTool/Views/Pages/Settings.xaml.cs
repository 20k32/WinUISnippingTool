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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUISnippingTool.Models.PageParameters;
using WinUISnippingTool.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class Settings : Page
{
    private Frame mainFrame;


    public SettingsWindowViewModel ViewModel { get; }
    public Settings()
    {
        this.InitializeComponent();
        ViewModel = new();
    }


    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if(e.Parameter is SettingsPageParameter settingsParameter)
        {
            mainFrame = settingsParameter.MainFrame;
            await ViewModel.LoadState(settingsParameter.BcpTag, settingsParameter.SaveImageLocation);
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {

        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = ViewModel.SelectedLanguageKind.BcpTag;
        mainFrame.BackStack.Clear();

        mainFrame.Navigate(typeof(MainPage), new SettingsPageParameter()
        {
            BcpTag = ViewModel.SelectedLanguageKind.BcpTag,
            MainFrame = mainFrame,
            SaveImageLocation = ViewModel.SaveImageLocation
        });
    }
}
