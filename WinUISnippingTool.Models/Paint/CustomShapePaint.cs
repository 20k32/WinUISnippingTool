using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml;
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
using Windows.Security.Credentials;
using Windows.Services.Maps;

namespace WinUISnippingTool.Models.Paint;

public class CustomShapePaint : SnipPaintBase
{
    private Point previousPosition;
    private readonly SolidColorBrush strokeColor;
    private ImageBrush fillColor;
    private Polyline polyline;
    private TranslateTransform translateTransform;

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
                Fill = fillColor,
                StrokeThickness = 4,
                FillRule = FillRule.EvenOdd,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
             var widthCoeff = WindowSize.Width / 3;
             var heightCoeff = WindowSize.Height / 3;
             int column = (int)(position.X / widthCoeff);
             int row = (int)(position.Y / heightCoeff);

            AlignmentX alignmentX = (AlignmentX)column;
            AlignmentY alignmentY = (AlignmentY)row;

            /* alignmentX = AlignmentX.Left;
             alignmentY = AlignmentY.Top;*/

            fillColor.Stretch = Stretch.None;
            fillColor.AlignmentX = AlignmentX.Left;
            fillColor.AlignmentY = AlignmentY.Top;

            previousPosition = position;
            StartPoint = position;

            Shapes.Add(polyline);

             translateTransform.X = -position.X;
             translateTransform.Y = 0;

            /*translateTransform.X = 0;
            translateTransform.Y = 0;*/
        }
    }


    // todo: finish this
    public override void OnPointerMoved(Point position)
    {
        if (IsDrawing 
            && CalculateDistance(previousPosition, position) > MinRenderDistance)
        {
            polyline.Points.Add(position);

           /* if (StartPoint.X < position.X
               && StartPoint.Y < position.Y)
            {
                translateTransform.X = StartPoint.X;
                Debug.WriteLine("Bottom right");
            }
            else if (StartPoint.X < position.X
                    && StartPoint.Y > position.Y)
            {
                translateTransform.X = StartPoint.X;
                Debug.WriteLine("Top right");
            }*/
            if (StartPoint.X > position.X
                    && StartPoint.Y < position.Y)
            {
                translateTransform.X = -StartPoint.X;

                Debug.WriteLine("Bottom left");
            }
            else if (StartPoint.X > position.X
                    && StartPoint.Y > position.Y)
            {
                translateTransform.X = -StartPoint.X;

                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} Top left");
            }
            else if(StartPoint.X > position.X && StartPoint.Y == position.Y)
            {
                translateTransform.X = -StartPoint.X;
            }
            else if (StartPoint.Y > position.Y && StartPoint.X == position.X)
            {
                translateTransform.Y = -StartPoint.Y;
            }

            StartPoint = position;

            previousPosition = position;
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

    public override void SetImageFill(ImageSource source)
    {
        fillColor = new ImageBrush()
        {
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            Opacity = 1,
            Transform = translateTransform
        };

        fillColor.ImageSource = source;
    }
}
