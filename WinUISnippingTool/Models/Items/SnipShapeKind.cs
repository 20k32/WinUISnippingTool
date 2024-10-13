using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUISnippingTool.Models.Items;

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

    public override bool Equals(object obj)
    {
        bool result = false;

        if(obj is SnipShapeKind shape)
        {
            result = shape.Kind == Kind;
        }

        return result;
    }

    /// <summary>
    /// If you want to store many entitites with same kinds, replace this method with better
    /// </summary>
    public override int GetHashCode() => Kind.GetHashCode();
}
