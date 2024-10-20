using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Security.Credentials;
using Windows.Services.Maps;

namespace WinUISnippingTool.Models.Paint;

public class CustomShapePaint : SnipPaintBase
{
    private Point thisPosition;
    private Point previousPosition;
    private readonly SolidColorBrush strokeColor;
    private SolidColorBrush fillColor;
    private Polyline polyline;
    private TranslateTransform translateTransform;

    private double deltaX;
    private double deltaY;

    public CustomShapePaint() : base()
    {
        translateTransform = new();
        strokeColor = new SolidColorBrush(Colors.White);
    }

    public override void OnPointerPressed(Point position)
    {
        if (!IsDrawing)
        {
            IsDrawing = true;

            polyline = new()
            {
                Stroke = strokeColor,
                StrokeThickness = 4,
                Fill = fillColor,
                FillRule = FillRule.EvenOdd,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };

            thisPosition = position;
            previousPosition = position;
            StartPoint = position;

            Shapes.Add(polyline);

            //translateTransform.X = -StartPoint.X;
            //translateTransform.Y = -StartPoint.Y;

            //polyline.RenderTransformOrigin = new Point(0, 0);
            
            /*deltaX = -StartPoint.X - 100;
            deltaY = -StartPoint.Y - 100;*/
        }
    }


    // todo: finish this
    public override void OnPointerMoved(Point position)
    {
        if (IsDrawing 
            && CalculateDistance(previousPosition, position) > MinRenderDistance)
        {
            
             polyline.Points.Add(position);
        }
    }

    public override Shape OnPointerReleased(Point position)
    {
        Polyline result = null;
        
        if(polyline is not null)
        {
            if (polyline.Points.Count > 1)
            {
                polyline.StrokeThickness = 0;
                result = polyline;
            }
            else
            {
                Shapes.Remove(polyline);
            }
        }

        IsDrawing = false;
        
        return result;
    }

    public override void Clear()
    {
        polyline = null;
        Shapes.Clear();
    }

    public override void SetImageFill(ImageSource _)
    {
        fillColor = new SolidColorBrush(Colors.Transparent);
    }
}
