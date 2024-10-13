using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;

namespace WinUISnippingTool.Models.Paint;

internal abstract class SnipPaintBase : PaintBase
{
    public Point StartPoint { get; protected set; }
    public SizeInt32 ActualSize { get; protected set; }

    protected static Size WindowSize;

    protected SnipPaintBase() : base(null)
    {
    }

    public void SetWindowSize(Size windowSize) => WindowSize = windowSize;

    public void SetShapeSource(NotifyOnCompletionCollection<UIElement> shapes) => Shapes = shapes;

    public abstract void SetImageFill(ImageSource source);
}
