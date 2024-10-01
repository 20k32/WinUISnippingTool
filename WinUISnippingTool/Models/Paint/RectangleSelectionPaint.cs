using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Paint
{
    internal sealed class RectangleSelectionPaint : SnipPaintBase
    {
        private readonly TranslateTransform translateTransform;
        private readonly ScaleTransform scaleTransform;
        private readonly SolidColorBrush strokeColor;
        private ImageBrush fillColor;

        private Point firstPosition;
        private Point previousPosition;
        private Path rect;

        public RectangleSelectionPaint() : base()
        {
            scaleTransform = new();
            strokeColor = new SolidColorBrush(Colors.White);
            translateTransform = new();
        }

        public override void OnPointerPressed(Point position)
        {
            if(!IsDrawing)
            {
                IsDrawing = true;
                
                scaleTransform.ScaleX = 0;
                scaleTransform.ScaleY = 0;

                rect = new Path()
                {
                    Fill = fillColor,
                    Stroke = strokeColor,
                    StrokeThickness = 1,
                    Data = new RectangleGeometry()
                    {
                        Transform = scaleTransform,
                        Rect = new Rect(0, 0, 1, 1)
                    },
                };
                
                firstPosition = position;
                previousPosition = position;
                Canvas.SetLeft(rect, position.X - 1);
                Canvas.SetTop(rect, position.Y - 1);
                Shapes.Add(rect);
            }
        }

        public override void OnPointerMoved(Point position)
        {
            if (IsDrawing
                && CalculateDistance(previousPosition, position) > MinRenderDistance)
            {
                double distanceX = position.X - firstPosition.X;
                double distanceY = position.Y - firstPosition.Y;

                double scaleX = 1 + (distanceX / 1);
                double scaleY = 1 + (distanceY / 1); // 1 <- can be presented in settings

                scaleTransform.ScaleX = scaleX;
                scaleTransform.ScaleY = scaleY;

                if(firstPosition.X < position.X
                   && firstPosition.Y < position.Y)
                {
                    translateTransform.X = -firstPosition.X;
                    translateTransform.Y = -firstPosition.Y;
                }
                else if(firstPosition.X < position.X
                        && firstPosition.Y > position.Y)
                {
                    translateTransform.X = -firstPosition.X; 
                    translateTransform.Y = -position.Y + 1;
                }
                else if(firstPosition.X > position.X
                        && firstPosition.Y < position.Y)
                {
                    translateTransform.X = -position.X + 1;
                    translateTransform.Y = -firstPosition.Y;
                }
                else if(firstPosition.X > position.X
                        && firstPosition.Y > position.Y)
                {
                    translateTransform.X = -position.X + 1;
                    translateTransform.Y = -position.Y + 1;
                }

                previousPosition = position;
            }
        }

        public override Shape OnPointerReleased(Point position)
        {
            Path result = null;
            var absX = Math.Abs(position.X - firstPosition.X);
            var absY = Math.Abs(position.Y - firstPosition.Y);

            if(absX > MinRenderDistance
                && absY > MinRenderDistance)
            {
                rect.StrokeThickness = 0;
                result = rect;
            }
            else
            {
                Shapes.Remove(rect);
                
            }

            IsDrawing = false;

            return result;
        }

        public override void Clear()
        {
            rect = null;
            Shapes.Clear();
        }

        public override void SetImageFill(ImageSource source)
        {
            fillColor = new ImageBrush()
            {
                ImageSource = source,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Transform = translateTransform
            };

            fillColor.Opacity = 1;
        }
    }
}
