using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Extensions;
using WinUISnippingTool.Models.Items;

namespace WinUISnippingTool.ViewModels
{
    internal sealed partial class SettingsWindowViewModel : ViewModelBase
    {
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
                new("Ukraininan", "uk-UA"),
                new("English", "en-US")
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
    }
}
