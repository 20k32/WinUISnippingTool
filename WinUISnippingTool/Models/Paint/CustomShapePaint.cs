using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Paint
{
    internal class CustomShapePaint : PaintBase
    {
        Point previousPosition;
        private PolyLineSegment polyLineSegment;
        private SolidColorBrush strokeColor;
        private SolidColorBrush fillColor;
        private Path userFigure;

        public CustomShapePaint(NotifyOnCompleteAddingCollection<UIElement> shapes) : base(shapes)
        {
            polyLineSegment = new();
            strokeColor = new SolidColorBrush(Colors.White);
            fillColor = new SolidColorBrush(Colors.DarkGray);
            fillColor.Opacity = 0.2;
        }
        public override void OnPointerPressed(Point position)
        {
            if (!IsDrawing)
            {
                IsDrawing = true;
                previousPosition = position;
                polyLineSegment.Points.Add(position);

                var segmentCollection = new PathSegmentCollection();
                segmentCollection.Add(polyLineSegment);

                var pathFigure = new PathFigure();
                pathFigure.StartPoint = position;
                pathFigure.Segments = segmentCollection;

                var figureCollection = new PathFigureCollection();
                figureCollection.Add(pathFigure);

                var geometry = new PathGeometry();
                geometry.Figures = figureCollection;

                userFigure = new Path();
                userFigure.Stroke = strokeColor;
                userFigure.Fill = fillColor;
                userFigure.StrokeThickness = 4;
                userFigure.Data = geometry;
                Shapes.Add(userFigure);
            }
        }

        public override void OnPointerMoved(Point position)
        {
            if (IsDrawing && CalculateDistance(previousPosition, position) > MinRenderDistance)
            {
                polyLineSegment.Points.Add(position);
            }
        }

        public override Shape OnPointerReleased(Point position)
        {
            IsDrawing = false;
            return userFigure;
        }
    }
}
