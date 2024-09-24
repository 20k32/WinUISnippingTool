using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Views;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel.DataTransfer;
using System;

namespace WinUISnippingTool.ViewModels;

internal sealed class MainWindowViewModel : CanvasViewModelBase
{
    private SnipScreenWindow snipScreen;
    private bool previousImageExists;
    private ScaleTransformManager transformManager;
    public NotifyOnCompleteAddingCollection<SnipShapeKind> SnipShapeKinds { get; private set; }

    public MainWindowViewModel() : base()
    {
        snipScreen = new();
        transformManager = new();
        SnipShapeKinds = new();
        CanvasWidth = 100;
        CanvasHeight = 100;
        Initialize();
        TrySetAndLoadLocalization("uk-UA");
        previousImageExists = false;

        snipScreen.Closed += async (x, args) =>
         {
             CanvasItems.Clear();
             var vm = ((SnipScreenWindowViewModel)snipScreen.ViewModel);
             var content = Clipboard.GetContent();
             var bitmap = await content.GetBitmapAsync();
             using (var stream = await bitmap.OpenReadAsync())
             {
                 var bitmapImage = new BitmapImage();
                 bitmapImage.SetSource(stream);
                 CanvasItems.Add(new Image { Source = bitmapImage});
             }

             CanvasWidth = vm.ResultFigureActualWidth;
             CanvasHeight = vm.ResultFigureActualHeight;

             SetTransformObjectSize(new(CanvasWidth, CanvasHeight));
             SetScaleCenterCoords(new(CanvasWidth, CanvasHeight));
         };
    }

    public ScaleTransform TransformSource => transformManager.TransfromSource;

    public void EnterSnippingMode(bool byShortcut)
    {
        var bitmapImage = ScreenshotHelper.GetBitmapImageScreenshotForArea(2560, 1440);
        snipScreen.SetBitmapImage(bitmapImage);
        snipScreen.SetResponceType(byShortcut);
        snipScreen.DefineKind(SelectedSnipKind.Kind);
       
        //snipScreen.PrepareWindow();
        snipScreen.Activate();
    }

    public void AddImage(Image image, int width, int height)
    {
        if (previousImageExists)
        {
            var existing = (Image)CanvasItems.First(x => x.GetType() == typeof(Image));
            CanvasItems.Remove(existing);
            previousImageExists = false;
        }
        else
        {
            previousImageExists = true;
        }
        CanvasItems.Add(image);
        var bitmapImage = ((BitmapImage)image.Source);

        CanvasWidth = width;
        CanvasHeight = height;

        SetTransformObjectSize(new(CanvasWidth, CanvasHeight));
        SetScaleCenterCoords(new(CanvasWidth, CanvasHeight));
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
