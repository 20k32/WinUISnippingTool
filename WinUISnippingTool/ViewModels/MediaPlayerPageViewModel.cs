using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.ViewModels;

internal class MediaPlayerPageViewModel : ViewModelBase
{
    private Uri uri;
    public Uri Uri
    {
        get => uri;
        set  
        {
            if(uri != value)
            {
                uri = value;
                NotifyOfPropertyChange();
            }
        }
    }

    protected override void LoadLocalization(string bcpTag)
    { }
}
