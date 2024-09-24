using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;

namespace WinUISnippingTool.Models.Paint
{
    internal sealed class WindowPaint : PaintBase
    {
        private bool isSelected;
        private Rectangle rect;
        private Size windowSize;
        public WindowPaint(NotifyOnCompleteAddingCollection<UIElement> shapes, Size windowSize, ImageSource source) : base(shapes)
        {
            this.windowSize = windowSize;
            rect = new()
            {
                Width = windowSize.Width,
                Height = windowSize.Height,
                StrokeThickness = 0,
                Fill = new ImageBrush
                {
                    ImageSource = source
                }
            };
            isSelected = false;
        }

        public override void OnPointerPressed(Point position)
        {
            if (position.X <= windowSize.Width
                && position.Y <= windowSize.Height)
            {
                if (!isSelected)
                {
                    Shapes.Add(rect);
                    isSelected = true;
                }
            }
        }

        public override void OnPointerMoved(Point position)
        {
            if (isSelected && (position.X == windowSize.Width
                || position.Y == windowSize.Height))
            {
                Shapes.Remove(rect);
                isSelected = false;
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
    }
}
