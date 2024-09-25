using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using WinUISnippingTool.Models.Paint;

namespace WinUISnippingTool.Models.Draw
{
    internal sealed class MarkerBrush : DrawBase
    {
        private SolidColorBrush drawingColor;
        private double drawingThickness;
        private Polyline line;
        private Point previousPosition;

        public MarkerBrush(NotifyOnCompletionCollection<UIElement> shapes, SolidColorBrush drawingColor, double drawingThickness) 
            : base(shapes, drawingColor, drawingThickness)
        {
            this.drawingColor = drawingColor;
            this.drawingThickness = drawingThickness;
        }

        public override void OnPointerPressed(Point position)
        {
            if (!IsDrawing)
            {
                IsDrawing = true;

                line = new Polyline()
                {
                    Stroke = drawingColor,
                    StrokeThickness = drawingThickness,
                    Opacity = 0.45
                };

                line.Points.Add(position);
                Shapes.Add(line);

                previousPosition = position;
            }
            
        }

        public override void OnPointerMoved(Point position)
        {
            if (IsDrawing)
            {
                if (CalculateDistance(previousPosition, position) > MinRenderDistance)
                {
                    line.Points.Add(position);
                    previousPosition = position;
                }
            }
        }

        public override Shape OnPointerReleased(Point position)
        {
            IsDrawing = false;
            return line;
        }
    }
}
