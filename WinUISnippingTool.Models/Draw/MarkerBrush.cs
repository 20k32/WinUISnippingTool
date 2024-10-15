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

namespace WinUISnippingTool.Models.Draw;

public sealed class MarkerBrush : DrawBase
{
    private Point previousPosition;

    public MarkerBrush(NotifyOnCompletionCollection<UIElement> shapes) 
        : base(shapes)
    { }

    public override void OnPointerPressed(Point position)
    {
        if (!IsDrawing)
        {
            IsDrawing = true;

            Line = new Polyline()
            {
                Stroke = DrawingColor,
                StrokeThickness = DrawingThickness,
                Opacity = 0.5,
                StrokeLineJoin = PenLineJoin.Bevel,
                StrokeStartLineCap = PenLineCap.Square,
                StrokeEndLineCap = PenLineCap.Square,
            };
            

            Line.Points.Add(position);
            Shapes.Add(Line);

            previousPosition = position;
        }
        
    }

    public override void OnPointerMoved(Point position)
    {
        if (IsDrawing)
        {
            if (CalculateDistance(previousPosition, position) > MinRenderDistance)
            {
                Line.Points.Add(position);
                previousPosition = position;
            }
        }
    }

    public override Shape OnPointerReleased(Point position)
    {
        IsDrawing = false;
        return Line;
    }
}
