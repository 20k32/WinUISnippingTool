using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Draw;

public sealed class SimpleBrush : DrawBase
{
    Point previousPosition;
    protected override int MinRenderDistance => 5;

    public SimpleBrush(NotifyOnCompletionCollection<UIElement> shapes) 
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
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };

            ((Polyline)Line).Points.Add(position);
            Shapes.Add(Line);
            previousPosition = position;
        }
    }

    public override void OnPointerMoved(Point position)
    {
        if (IsDrawing)
        {
            if(CalculateDistance(previousPosition, position) > MinRenderDistance)
            {
                ((Polyline)Line).Points.Add(position);
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
