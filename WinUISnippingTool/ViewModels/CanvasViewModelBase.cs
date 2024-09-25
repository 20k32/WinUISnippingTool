using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;

namespace WinUISnippingTool.ViewModels;

internal abstract class CanvasViewModelBase : INotifyPropertyChanged
{
    protected Size defaultWindowSize = new(500, 500);
    public NotifyOnCompletionCollection<SnipShapeKind> SnipShapeKinds { get; private set; }

    protected CanvasViewModelBase()
    {
        CanvasItems = new();
        SnipShapeKinds = new();
        SnipShapeKinds.AddRange(new SnipShapeKind[]
       {
            new(string.Empty, "\uF407", SnipKinds.Recntangular),
            new(string.Empty, "\uF7ED", SnipKinds.Window),
            new(string.Empty, "\uE7F4", SnipKinds.AllWindows),
            new(string.Empty, "\uF408", SnipKinds.CustomShape)
       });
    }

    protected void TrySetAndLoadLocalization(string bcpTag)
    {
        if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != bcpTag)
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = bcpTag;
        }
        var languages = Windows.Globalization.ApplicationLanguages.Languages;

        var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        SnipShapeKinds[0].Name = resourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[1].Name = resourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[2].Name = resourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[3].Name = resourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
    }

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
                SelectionChangedCallback();
            }
        }
    }

    protected virtual void SelectionChangedCallback()
        => NotifyOfPropertyChange(nameof(SelectedSnipKind));

    #endregion

    public virtual void SetWindowSize(Size newSize)
    {
        defaultWindowSize = newSize;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected Microsoft.UI.Xaml.Controls.Image currentImage;
    public NotifyOnCompletionCollection<UIElement> CanvasItems { get; protected set; }

    private double canvasWidth;
    public double CanvasWidth
    {
        get => canvasWidth;
        set
        {
            if (canvasWidth != value)
            {
                canvasWidth = value;
                NotifyOfPropertyChange();
            }
        }
    }

    private double canvasHeight;
    public double CanvasHeight
    {
        get => canvasHeight;
        set
        {
            if (canvasHeight != value)
            {
                canvasHeight = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
