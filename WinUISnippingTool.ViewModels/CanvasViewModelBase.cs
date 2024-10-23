using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using System.Diagnostics;

namespace WinUISnippingTool.ViewModels;

public abstract class CanvasViewModelBase : ViewModelBase
{
    protected Size DefaultWindowSize = new(500, 500);
    public NotifyOnCompletionCollection<SnipShapeKind> SnipShapeKinds { get; protected set; }

    protected internal CanvasViewModelBase()
    {
        SnipShapeKinds = new();

        SnipShapeKinds.AddRange(
        [
            new(string.Empty, "\uF407", SnipKinds.Recntangular),
            new(string.Empty, "\uF7ED", SnipKinds.Window),
            new(string.Empty, "\uE7F4", SnipKinds.AllWindows),
            new(string.Empty, "\uF408", SnipKinds.CustomShape)
        ]);
    }

    protected override void LoadLocalization(string bcpTag)
    {
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = bcpTag;

        SnipShapeKinds[0].Name = ResourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[1].Name = ResourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[2].Name = ResourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[3].Name = ResourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
    }

    #region Selected snip kind

    private SnipShapeKind selectedSnipKind;

    public SnipShapeKind SelectedSnipKind
    {
        get => selectedSnipKind;
        set
        {
            if (value is not null 
                && (selectedSnipKind is null 
                    || !selectedSnipKind.Equals(value)))
            {
                selectedSnipKind = value;
                SelectionChangedCallback();
            }
        }
    }

    protected virtual void SelectionChangedCallback()
        => NotifyOfPropertyChange(nameof(SelectedSnipKind));

    #endregion

    private CaptureType captureType;
    public CaptureType CaptureType
    {
        get => captureType;
        set
        {
            if (captureType != value)
            {
                captureType = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public virtual void SetWindowSize(Size newSize)
    {
        DefaultWindowSize = newSize;
    }

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
