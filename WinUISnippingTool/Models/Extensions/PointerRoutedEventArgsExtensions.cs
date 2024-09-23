using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.ConversationalAgent;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Extensions
{
    internal static class PointerRoutedEventArgsExtensions
    {
        public static Point GetPositionRelativeToCanvas(this PointerRoutedEventArgs args, Canvas canvas)
        {
            Point result = default;

            try
            {
                result = args.GetCurrentPoint(canvas).Position;
            }
            catch
            { }

            return result;
        }
    }
}
