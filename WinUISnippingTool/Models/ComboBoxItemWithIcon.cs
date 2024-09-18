using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models
{
    internal sealed class ComboBoxItemWithIcon
    {
        public string Name { get; set; }
        public string Glyph { get; set; }

        public ComboBoxItemWithIcon(string name, string glyph) =>
            (Name, Glyph) = (name, glyph);
    }
}
