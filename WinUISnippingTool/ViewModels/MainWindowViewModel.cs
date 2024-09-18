using System;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;

namespace WinUISnippingTool.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Initialize();
        TrySetAndLoadLocalization("en-US");
    }

    public NotifyOnCompleteAddingCollection<SnipShapeKind> SnipKinds { get; private set; }

    #region Selected snip kind

    private SnipShapeKind selectedSnipKind;

    public SnipShapeKind SelectedSnipKind
    {
        get => selectedSnipKind;
        set
        {
            if (selectedSnipKind != value)
            {
                selectedSnipKind = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion

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

    private void TrySetAndLoadLocalization(string bcpTag)
    {
        if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != bcpTag)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = bcpTag;
        }

        var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        TakePhotoButtonName = resourceMap.GetValue("TakePhotoButtonName/Text")?.ValueAsString ?? "empty_value";
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
