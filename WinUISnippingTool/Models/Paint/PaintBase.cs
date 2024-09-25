using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;


namespace WinUISnippingTool.Models.Paint;

internal abstract class PaintBase
{
    protected bool IsDrawing;

    protected virtual int MinRenderDistance => 10;
    public Shape TemporaryResultShape = null!;

    protected NotifyOnCompletionCollection<UIElement> Shapes; 
    protected PaintBase(NotifyOnCompletionCollection<UIElement> shapes)
    {
        Shapes = shapes;
        IsDrawing = false;
    }

    public abstract void OnPointerPressed(Point position);
    public abstract void OnPointerMoved(Point position);
    public abstract Shape OnPointerReleased(Point position);
    public abstract void Clear();

    protected double CalculateDistance(Point a, Point b) =>
            Math.Sqrt(Math.Pow((b.X - a.X), 2) +
                Math.Pow((b.Y - a.Y), 2));
}
