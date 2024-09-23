using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WinUISnippingTool.Models;

namespace WinUISnippingTool.ViewModels;

internal abstract class CanvasViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected CanvasViewModelBase()
    {
        CanvasItems = new();
    }

    protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected Microsoft.UI.Xaml.Controls.Image currentImage;
    public NotifyOnCompleteAddingCollection<UIElement> CanvasItems { get; protected set; }

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
