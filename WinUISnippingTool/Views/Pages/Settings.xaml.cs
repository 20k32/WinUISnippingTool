using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
    private SettingsWindowViewModel ViewModel { get; }
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
            await ViewModel.LoadStateAsync(settingsParameter);
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        var comparisonBcp = ViewModel.SelectedLanguageKind.BcpTag.Substring(0, 2).ToLower();

        if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != comparisonBcp)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = ViewModel.SelectedLanguageKind.BcpTag;
            Frame.BackStack.Clear();
        }
        
        Frame.Navigate(typeof(MainPage), new SettingsPageParameter(ViewModel.SelectedLanguageKind.BcpTag, ViewModel.SaveImageLocation, ViewModel.SaveVideoLocation));
    }
}
