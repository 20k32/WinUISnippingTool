using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUISnippingTool.Models.TemplateSelectors
{
    internal sealed class ComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedItemTemplate { get; set; }
        public DataTemplate DropdownItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var isDropDown = container is ComboBoxItem;

            return isDropDown
                ? DropdownItemTemplate
                : SelectedItemTemplate;
        }
    }
}
