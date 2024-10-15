using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Extensions;

public static class PointerRoutedEventArgsExtensions
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
