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

namespace WinUISnippingTool.ViewModels
{
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

        private string changeLocationButtonName;
        public string ChangeLocationButtonName
        {
            get => changeLocationButtonName;
            set
            {
                if (changeLocationButtonName != value)
                {
                    changeLocationButtonName = value;
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


        public async Task LoadState(string bcpTag, StorageFolder saveImageLocation)
        {
            LoadLocalization(bcpTag);

            SelectedLanguageKind = Languages.FirstOrDefault(lang => lang.BcpTag == bcpTag);

            if(saveImageLocation is null)
            {
                SaveImageLocation = await PicturesFolderExtensions.GetScreenshotsFolderAsync();
            }
            else
            {
                SaveImageLocation = saveImageLocation;
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

        protected override void LoadLocalization(string bcpTag)
        {
            BackButtonName = resourceMap.GetValue("BackButton/Text")?.ValueAsString ?? "empty_value";
            ChangeLocationButtonName = resourceMap.GetValue("ChangeLocationButton/Text")?.ValueAsString ?? "empty_value";
            Languages[1].DisplayName = resourceMap.GetValue("EnglishLangMenuItem/Text")?.ValueAsString ?? "empty_value";
            Languages[0].DisplayName = resourceMap.GetValue("UkrainianLangMenuItem/Text")?.ValueAsString ?? "empty_value";
        }
    }
}
