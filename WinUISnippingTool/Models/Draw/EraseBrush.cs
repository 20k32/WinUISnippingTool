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
    internal sealed class EraseBrush : DrawBase
    {
        public EraseBrush(NotifyOnCompletionCollection<UIElement> shapes) : base(shapes)
        { }

        public override void OnPointerPressed(Point position)
        {
            if (!IsDrawing)
            {
                foreach(Shape item in Shapes.Skip(1))
                {
                    AddEraseHandler(item);
                }
                IsDrawing = true;
            }
        }

        public override void OnPointerMoved(Point position)
        { }

        public override Shape OnPointerReleased(Point position)
        {
            IsDrawing = false;
            return null;
        }
    }
}
