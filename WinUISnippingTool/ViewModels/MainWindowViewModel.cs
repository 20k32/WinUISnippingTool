using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Views;
using Microsoft.UI.Xaml.Media;

namespace WinUISnippingTool.ViewModels;

internal sealed class MainWindowViewModel : CanvasViewModelBase
{

    private ScaleTransformManager transformManager;
    public NotifyOnCompleteAddingCollection<SnipShapeKind> SnipShapeKinds { get; private set; }

    public MainWindowViewModel() : base()
    {
        transformManager = new();
        SnipShapeKinds = new();
        CanvasWidth = 100;
        CanvasHeight = 100;
        Initialize();
        TrySetAndLoadLocalization("uk-UA");
    }

    public ScaleTransform TransformSource => transformManager.TransfromSource;

    public async Task EnterSnippingMode()
    {
        //Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";

        //var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        //SnipKinds[0].Name = resourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        //SnipKinds[1].Name = resourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        //SnipKinds[2].Name = resourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        //SnipKinds[3].Name = resourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
        //var bitmapImage = new BitmapImage();

        //var bitmapImage = ScreenshotHelper.GetBitmapImageScreenshotForArea(500, 500);
        //CanvasWidth = bitmapImage.PixelHeight;
        //CanvasHeight = bitmapImage.PixelWidth;

        //if (currentImage is not null)
        //{
        //    CanvasItems.Remove(currentImage);
        //}

        //var image = new Microsoft.UI.Xaml.Controls.Image();
        //image.Source = bitmapImage;
        //CanvasItems.Add(image);
        //currentImage = image;
        //var newSize = new Windows.Foundation.Size(CanvasWidth, CanvasHeight);
        //SetTransformObjectSize(newSize);
        
        var bitmapImage = ScreenshotHelper.GetBitmapImageScreenshotForArea(2560, 1440);
        var window = new SnipScreenWindow(bitmapImage, SelectedSnipKind.Kind);
        window.Activate();
    }

    public void SetTransformObjectSize(Windows.Foundation.Size transformObject) 
        => transformManager.SetTransformObject(transformObject);

    public void SetRelativeObjectSize(Windows.Foundation.Size relativeObject) 
        => transformManager.SetRelativeObject(relativeObject);

    public void SetScaleCenterCoords(Windows.Foundation.Size size)
        => transformManager.SetScaleCenterCoords(size);

    public void Transform(Windows.Foundation.Size relativeTo) => transformManager.Transform(relativeTo);
    public void Transform() => transformManager.Transform();

    public void ResetTransform() => transformManager.ResetTransform();

    private void Initialize()
    {
        SnipShapeKinds.AddRange(new SnipShapeKind[]
        {
            new(string.Empty, "\uF407", SnipKinds.Recntangular),
            new(string.Empty, "\uF7ED", SnipKinds.Window),
            new(string.Empty, "\uE7F4", SnipKinds.AllWindows),
            new(string.Empty, "\uF408", SnipKinds.CustomShape)
        });

        SelectedSnipKind = SnipShapeKinds.First();
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
        SnipShapeKinds[0].Name = resourceMap.GetValue("RectangleAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[1].Name = resourceMap.GetValue("WindowAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[2].Name = resourceMap.GetValue("FullScreenAreaName/Text")?.ValueAsString ?? "emtpy_value";
        SnipShapeKinds[3].Name = resourceMap.GetValue("FreeFormAreaName/Text")?.ValueAsString ?? "emtpy_value";
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
