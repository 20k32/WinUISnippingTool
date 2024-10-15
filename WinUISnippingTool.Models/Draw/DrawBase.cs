using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;
using WinUISnippingTool.Models.Paint;

namespace WinUISnippingTool.Models.Draw;

public abstract class DrawBase : PaintBase
{
    private static readonly Dictionary<string, SolidColorBrush> usedBrushes;
    private static readonly Stack<UIElement> shapeStack;
    protected SolidColorBrush DrawingColor;
    protected double DrawingThickness;
    protected Polyline Line;

    static DrawBase()
    {
        shapeStack = new();
        usedBrushes = new();
    }

    protected DrawBase(NotifyOnCompletionCollection<UIElement> shapes) : base(shapes)
    {}
    
   

    public void UndoGlobal()
    {
        var lastValue = Shapes.LastOrDefault();

        if(lastValue is not null
            && lastValue is not Image)
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

    public override void Clear()
    {
        var tempArr = Shapes.Skip(1).ToArray();
        
        foreach (Shape item in tempArr.Cast<Shape>())
        {
            DetachEraseHandler(item);
            Shapes.Remove(item);
        }

        foreach (Shape item in shapeStack.Cast<Shape>())
        {
            DetachEraseHandler(item);
        }

        shapeStack.Clear();
    }

    public void SetColorHex(string hex)
    {
        if (usedBrushes.TryGetValue(hex, out var solidBrush))
        {
            DrawingColor = solidBrush;
        }
        else
        {
            var color = Color.FromArgb(255,
               byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber),
               byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber),
               byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber));

            DrawingColor = new(color);
            usedBrushes.Add(hex, DrawingColor);
        }
    }

    public void SetDrawingThickness(double thickness)
    {
        DrawingThickness = thickness;
    }

    protected void AddEraseHandler(Shape line)
    {
        line.PointerMoved += Line_PointerMoved;
    }

    private void Line_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var element = (UIElement)sender;

        if (this is EraseBrush eraseBrush
            && IsDrawing
            && Shapes.Contains(element))
        {
            Shapes.Remove(element);
            shapeStack.Push(element);

            if(Shapes.Count == 1)
            {
                eraseBrush.NotifyUndoChanged();
            }

            Debug.WriteLine("Erased");
        }
    }

    private void DetachEraseHandler(Shape line)
    {
        line.PointerMoved -= Line_PointerMoved;
    }

    public bool CanUndo() => Shapes.Count > 0;
    public bool CanRedo() => shapeStack.Count > 0;
}
