using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Graphics.Capture;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.WindowManagement;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Views;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.Display;
using Windows.Foundation;

namespace WinUISnippingTool.ViewModels;

internal sealed partial class MainWindowViewModel : ViewModelBase
{

    private ScaleTransformManager transformManager;
    public NotifyOnCompleteAddingCollection<SnipShapeKind> SnipKinds { get; private set; }

    public MainWindowViewModel()
    {
        transformManager = new();
        Initialize();
        TrySetAndLoadLocalization("uk-UA");
    }

    public ScaleTransform GetTransformSource() => transformManager.TransfromSource;

    public void InputCommand()
    {
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";

        var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        SnipKinds[0].Name = resourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[1].Name = resourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[2].Name = resourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipKinds[3].Name = resourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
    }

    public void SetTransformObjectSize(Size transformObject) 
        => transformManager.SetTransformObject(transformObject);

    public void SetRelativeObjectSize(Size relativeObject) 
        => transformManager.SetRelativeObject(relativeObject);

    public void SetScaleSenterCoords(Size size)
        => transformManager.SetScaleCenterCoords(size);

    public void Transform(Size relativeTo) => transformManager.Transform(relativeTo);
    public void Transform() => transformManager.Transform();

    public void ResetTransform() => transformManager.ResetTransform();

    public void OpenSnipScreenWindow()
    {
        var newWindow = new SnipScreenWindow();
        newWindow.Activate();
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

    #region Relative object size

    private double relativeObjectWidth;
    public double RelativeObjectWidth
    {
        get => relativeObjectWidth;
        set
        {
            if(relativeObjectWidth != value)
            {
                relativeObjectWidth = value;
                NotifyOfPropertyChange();
            }
        }
    }

    private double relativeObjectHeight;
    public double RelativeObjectHeight
    {
        get => relativeObjectHeight;
        set
        {
            if(relativeObjectHeight != value)
            {
                relativeObjectHeight = value;
                NotifyOfPropertyChange();
            }
        }
    }

    #endregion

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
