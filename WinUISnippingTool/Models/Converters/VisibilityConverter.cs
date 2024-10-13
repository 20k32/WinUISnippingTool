using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models.Converters;

internal sealed class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isVisible)
        {
            var equalValue = bool.Parse(parameter.ToString());
            value = isVisible == equalValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) 
        => value;
}
