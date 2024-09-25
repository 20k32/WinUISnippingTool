using Microsoft.UI.Xaml;
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
    internal sealed class SimpleBrush : DrawBase
    {
        private SolidColorBrush drawingColor;
        private double drawingThickness;
        private Polyline line;
        Point previousPosition;
        protected override int MinRenderDistance => 5;


        public SimpleBrush(NotifyOnCompletionCollection<UIElement> shapes, SolidColorBrush drawingColor, double drawingThickness) 
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

                line = new()
                {
                    Stroke = drawingColor,
                    StrokeThickness = drawingThickness
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
                if(CalculateDistance(previousPosition, position) > MinRenderDistance)
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
