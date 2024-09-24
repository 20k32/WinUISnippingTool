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

internal abstract class DrawBase : PaintBase
{
    private Stack<UIElement> shapeStack;
    protected SolidColorBrush DrawingColor;
    protected double DrawingThickness;
    protected bool IsDrawing;
    protected DrawBase(NotifyOnCompleteAddingCollection<UIElement> shapes, SolidColorBrush drawingColor, double drawingThickness) : base(shapes)
    {
        this.DrawingColor = drawingColor;
        this.DrawingThickness = drawingThickness;
        shapeStack = new();
    }

    public void UndoGlobal()
    {
        var lastValue = Shapes.LastOrDefault();

        if(lastValue is not null)
        {
            Shapes.Remove(lastValue);
            shapeStack.Push(lastValue);
        }
    }
    public void RedoGlobal() 
    {
        if(shapeStack.Count > 0)
        {
            Shapes.Add(shapeStack.Pop());
        }
    }
}
