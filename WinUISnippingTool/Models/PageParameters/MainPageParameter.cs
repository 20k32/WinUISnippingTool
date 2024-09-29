using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models.PageParameters
{
    internal sealed class MainPageParameter : PageParameterBase
    {
        public OverlappedPresenter AppWindowPresenter;
        public DisplayArea CurrentDisplayArea;
    }
}
