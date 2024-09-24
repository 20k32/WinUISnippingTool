using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Draw
{
    internal sealed class Erase : DrawBase
    {
        private List<UIElement> linesToRemove;

        public Erase(NotifyOnCompleteAddingCollection<UIElement> shapes) : base(shapes, null, default)
        {
            linesToRemove = new();
        }

        public override void OnPointerPressed(Point position)
        {
            if (!IsDrawing)
            {
                IsDrawing = true;
            }
        }

        public override void OnPointerMoved(Point position)
        {
            if (IsDrawing) 
            {
                linesToRemove.Clear();

                foreach (var item in Shapes)
                {
                    if (item is Polyline line )
                    {
                        foreach(var point in line.Points)
                        {
                            if(point == position)
                            {
                                linesToRemove.Add(item);
                                continue;
                            }
                        }
                    }
                }

                foreach (var item in linesToRemove)
                {
                    Shapes.Remove(item);
                }
            }
        }

        public override Shape OnPointerReleased(Point position)
        {
            IsDrawing = false;
            return null;
        }
    }
}
