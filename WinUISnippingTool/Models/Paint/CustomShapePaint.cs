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
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Services.Maps;

namespace WinUISnippingTool.Models.Paint
{
    internal class CustomShapePaint : PaintBase
    {
        private Point previousPosition;
        private Size windowSize;
        private readonly SolidColorBrush strokeColor;
        private readonly ImageBrush fillColor;
        private readonly Polyline polyline;

        public CustomShapePaint(NotifyOnCompletionCollection<UIElement> shapes, ImageSource source) : base(shapes)
        {
            strokeColor = new SolidColorBrush(Colors.White);
            fillColor = new ImageBrush()
            {
                ImageSource = source,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Opacity = 1,
            };

            polyline = new()
            {
                Stroke = strokeColor,
                Fill = fillColor,
                StrokeThickness = 4,
                FillRule = FillRule.Nonzero
            };

            windowSize = new(2560, 1440);
        }
        public override void OnPointerPressed(Point position)
        {
            if (!IsDrawing)
            {
                IsDrawing = true;
                var widthCoeff = windowSize.Width / 3;
                var heightCoeff = windowSize.Height / 3;
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
            if (IsDrawing && CalculateDistance(previousPosition, position) > MinRenderDistance)
            {
                polyline.Points.Add(position);
                previousPosition = position;
            }
        }

        public override Shape OnPointerReleased(Point position)
        {
            IsDrawing = false;
            polyline.StrokeThickness = 0;
            return polyline;
        }

        public override void Clear()
        {
            Shapes.Clear();
        }
    }
}
