using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WinUISnippingTool.ViewModels;

internal abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
