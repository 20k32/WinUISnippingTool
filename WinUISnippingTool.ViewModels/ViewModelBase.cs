using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;

namespace WinUISnippingTool.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    protected static readonly ResourceMap ResourceMap;
    
    static ViewModelBase()
    {
        ResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
    }

    protected abstract void LoadLocalization(string bcpTag);

    public event PropertyChangedEventHandler PropertyChanged;
    protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
