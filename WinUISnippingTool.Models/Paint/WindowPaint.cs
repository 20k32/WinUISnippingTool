using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Paint;

public sealed class WindowPaint : SnipPaintBase
{
    Point firstPosition;
    private bool isSelected;
    private Rectangle rect;
    private ImageBrush fill;

    public WindowPaint() : base()
    {
        isSelected = false;
    }

    public override void OnPointerPressed(Point position)
    {
        if (position.X <= WindowSize.Width
            && position.Y <= WindowSize.Height)
        {
            rect = new()
            {
                StrokeThickness = 0,
                Fill = fill,
                Width = WindowSize.Width,
                Height = WindowSize.Height
            };

            Shapes.Add(rect);

            isSelected = true;

            firstPosition = position;
        }
    }

    public override void OnPointerMoved(Point position)
    {
        if (isSelected 
            && CalculateDistance(firstPosition, position) > MinRenderDistance)
        {
            isSelected = false;
            Shapes.Remove(rect);
        }
    }

    public override Shape OnPointerReleased(Point position)
    {
        Shape result = null;

        if (isSelected)
        {
            result = rect;
        }

        return result;
    }

    public override void Clear()
    {
        Shapes.Clear();
    }

    public override void SetImageFill(ImageSource source)
    {
        rect = null;
        fill = new ImageBrush
        {
            ImageSource = source
        };
    }
}
