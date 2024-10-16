using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;


namespace WinUISnippingTool.Helpers;

public static class UIElementExtensions
{
    private static UIElement cached;
   
    public static T FindAscendantCached<T>(this UIElement element) where T : UIElement
    {
        cached ??= element.FindAscendant<T>();

        return cached as T;
    }
}
