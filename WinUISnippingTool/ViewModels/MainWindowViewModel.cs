using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using Windows.ApplicationModel.Store;
using WinUISnippingTool.Views;
using Windows.ApplicationModel.Resources.Core;

namespace WinUISnippingTool.ViewModels;

internal sealed partial class MainWindowViewModel : ViewModelBase
{

    public NotifyOnCompleteAddingCollection<SnipShapeKind> SnipKinds { get; private set; }

    public MainWindowViewModel()
    {
        Initialize();
        TrySetAndLoadLocalization("uk-UA");
    }

    public void InputCommand()
    {
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
        var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        SnipKinds[0].Name = resourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[1].Name = resourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[2].Name = resourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[3].Name = resourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
    }

    private void Initialize()
    {
        SnipKinds = new();

        SnipKinds.AddRange(new SnipShapeKind[]
        {
            new(string.Empty, "\uF407"),
            new(string.Empty, "\uF7ED"),
            new(string.Empty, "\uE7F4"),
            new(string.Empty, "\uF408")
        });

        SelectedSnipKind = SnipKinds.First();
    }

    #region Selected snip kind

    private SnipShapeKind selectedSnipKind;

    public SnipShapeKind SelectedSnipKind
    {
        get => selectedSnipKind;
        set
        {
            selectedSnipKind = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    private void TrySetAndLoadLocalization(string bcpTag)
    {
        if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != bcpTag)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = bcpTag;
        }
        var languages = Windows.Globalization.ApplicationLanguages.Languages;

        var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        SnipKinds[0].Name = resourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[1].Name = resourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[2].Name = resourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[3].Name = resourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
    }

    #region Take photo button name

    private string takePhotoButtonName;
    public string TakePhotoButtonName
    {
        get => takePhotoButtonName;
        set
        {
            if (takePhotoButtonName != value)
            {
                takePhotoButtonName = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion
}
