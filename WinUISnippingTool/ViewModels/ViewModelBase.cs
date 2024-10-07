using Microsoft.UI.Xaml;
using Microsoft.Windows.System.Power;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Graphics.DirectX.Direct3D11;

namespace WinUISnippingTool.ViewModels
{
    internal abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected static readonly ResourceMap resourceMap;
        
        static ViewModelBase()
        {
            resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        }

        protected abstract void LoadLocalization(string bcpTag);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
