using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models.Items
{
    internal sealed class SnipShapeKind : ModelBase
    {
        public readonly SnipKinds Kind;

        #region Shape name
        private string name { get; set; }

        public string Name
        {
            get => name;
            set
            {
                if(name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Shape icon

        private string glyph;

        public string Glyph
        {
            get => glyph;
            set
            {
                if (glyph != value)
                {
                    glyph = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public SnipShapeKind(string name, string glyph, SnipKinds kind) =>
            (this.name, this.glyph, Kind) = (name, glyph, kind);
    }
}
