using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Models.PageParameters;

namespace WinUISnippingTool.ViewModels;

internal sealed partial class SettingsWindowViewModel : ViewModelBase
{
    private string backButtonName;
    public string BackButtonName
    {
        get => backButtonName;
        set
        {
            if (backButtonName != value)
            {
                backButtonName = value;
                NotifyOfPropertyChange();
            }
        }
    }

    private string changePhotoLocationButtonName;
    public string ChangePhotoLocationButtonName
    {
        get => changePhotoLocationButtonName;
        set
        {
            if (changePhotoLocationButtonName != value)
            {
                changePhotoLocationButtonName = value;
                NotifyOfPropertyChange();
            }
        }
    }

    private string changeVideoLocationButtonName;
    public string ChangeVideoLocationButtonName
    {
        get => changeVideoLocationButtonName;
        set
        {
            if (changeVideoLocationButtonName != value)
            {
                changeVideoLocationButtonName = value;
                NotifyOfPropertyChange();
            }
        }
    }


    private LanguageKind selectedLanguageKind;
    public LanguageKind SelectedLanguageKind
    {
        get => selectedLanguageKind;
        set
        {
            if(selectedLanguageKind != value)
            {
                selectedLanguageKind = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public NotifyOnCompletionCollection<LanguageKind> Languages { get; }

    public SettingsWindowViewModel()
    {
        Languages = new();
        Languages.AddRange(new LanguageKind[]
        {
            new(string.Empty, "uk-UA"),
            new(string.Empty, "en-US")
        });
    }

    private StorageFolder saveImageLocation;

    public StorageFolder SaveImageLocation
    {
        get => saveImageLocation;
        set
        {
            if(saveImageLocation != value)
            {
                saveImageLocation = value;
                NotifyOfPropertyChange();
            }
        }
    }

    private StorageFolder saveVideoLocation;

    public StorageFolder SaveVideoLocation
    {
        get => saveVideoLocation;
        set
        {
            if(saveVideoLocation != value)
            {
                saveVideoLocation = value;
                NotifyOfPropertyChange();
            }
        }
    }


    public async Task LoadStateAsync(SettingsPageParameter parameter)
    {
        LoadLocalization(parameter.BcpTag);

        SelectedLanguageKind = Languages.FirstOrDefault(lang => lang.BcpTag == parameter.BcpTag);

        if(parameter.SaveImageLocation is null)
        {
            SaveImageLocation = await FolderExtensions.GetDefaultScreenshotsFolderAsync();
        }
        else
        {
            SaveImageLocation = parameter.SaveImageLocation;
        }

        if(parameter.SaveVideoLocation is null)
        {
            SaveVideoLocation = await FolderExtensions.GetDefaultVideosFolderAsync();
        }
        else
        {
            SaveVideoLocation = parameter.SaveVideoLocation;
        }
    }

    [RelayCommand]
    private async Task PickFolderAsync()
    {
        var storageFolder = await FilePickerExtensions.ShowFolderPickerAsync();

        if (storageFolder is not null)
        {
            SaveImageLocation = storageFolder;
        }
    }

    [RelayCommand]
    private async Task PickFolderForVideo()
    {
        var storageFolder = await FilePickerExtensions.ShowFolderPickerAsync();

        if (storageFolder is not null)
        {
            SaveVideoLocation = storageFolder;
        }
    }

    protected override void LoadLocalization(string bcpTag)
    {
        BackButtonName = ResourceMap.GetValue("BackButton/Text")?.ValueAsString ?? "empty_value";
        ChangePhotoLocationButtonName = ResourceMap.GetValue("ChangePhotosLocationButton/Text")?.ValueAsString ?? "empty_value";
        ChangeVideoLocationButtonName = ResourceMap.GetValue("ChangeVideosLocationButton/Text")?.ValueAsString ?? "empty_value";
        Languages[1].DisplayName = ResourceMap.GetValue("EnglishLangMenuItem/Text")?.ValueAsString ?? "empty_value";
        Languages[0].DisplayName = ResourceMap.GetValue("UkrainianLangMenuItem/Text")?.ValueAsString ?? "empty_value";
    }
}
