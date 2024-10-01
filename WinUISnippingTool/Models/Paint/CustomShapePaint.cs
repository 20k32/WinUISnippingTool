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

namespace WinUISnippingTool.Models.Paint
{
    internal class CustomShapePaint : SnipPaintBase
    {
        private Point previousPosition;
        private readonly SolidColorBrush strokeColor;
        private ImageBrush fillColor;
        private Polyline polyline;

        public CustomShapePaint() : base()
        {
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

                AlignmentX alignmentX = AlignmentX.Center;
                AlignmentY alignmentY = AlignmentY.Center;

                
                switch (column)
                {
                    case 0:
                        alignmentX = AlignmentX.Left;
                        break;
                    case 1:
                        alignmentX = AlignmentX.Center;
                        break;
                    case 2:
                        alignmentX = AlignmentX.Right;
                        break;
                }

                
                switch (row)
                {
                    case 0:
                        alignmentY = AlignmentY.Top;
                        break;
                    case 1:
                        alignmentY = AlignmentY.Center;
                        break;
                    case 2:
                        alignmentY = AlignmentY.Bottom;
                        break;
                }

                fillColor.AlignmentX = alignmentX;
                fillColor.AlignmentY = alignmentY;

                previousPosition = position;

                Shapes.Add(polyline);
            }
        }

        public override void OnPointerMoved(Point position)
        {
            if (IsDrawing 
                && CalculateDistance(previousPosition, position) > MinRenderDistance)
            {
                polyline.Points.Add(position);
                previousPosition = position;
            }
        }

        public override Shape OnPointerReleased(Point position)
        {
            Polyline result = null;

            if (polyline.Points.Count > 1)
            {
                polyline.StrokeThickness = 0;
                result = polyline;
            }
            else
            {
                Shapes.Remove(polyline);
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
            };

            fillColor.ImageSource = source;
        }
    }
}
