using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.Items;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.UserControls
{
    internal sealed partial class ColorPicker : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor),
                typeof(ColorKind),
                typeof(ColorPicker),
                new(string.Empty));

        public ColorKind SelectedColor
        {
            get => (ColorKind)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty ColorListProperty =
            DependencyProperty.Register(nameof(ColorList),
                typeof(NotifyOnCompletionCollection<ColorKind>),
                typeof(ColorPicker),
                new(null));

        public NotifyOnCompletionCollection<ColorKind> ColorList
        {
            get => (NotifyOnCompletionCollection<ColorKind>)GetValue(ColorListProperty);
            set => SetValue(ColorListProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.RegisterAttached(nameof(StrokeThickness),
                typeof(double),
                typeof(ColorPicker),
                new(1));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty MinThicknessProperty =
            DependencyProperty.RegisterAttached(nameof(MinThickness),
                typeof(double),
                typeof(ColorPicker),
                new(1));

        public double MinThickness
        {
            get => (double)GetValue(MinThicknessProperty);
            set => SetValue(MinThicknessProperty, value);
        }

        public static readonly DependencyProperty MaxThicknessProperty =
            DependencyProperty.RegisterAttached(nameof(MaxThickness),
                typeof(double),
                typeof(ColorPicker),
                new(30));

        public double MaxThickness
        {
            get => (double)GetValue(MaxThicknessProperty);
            set => SetValue(MaxThicknessProperty, value);
        }

        public static readonly DependencyProperty ShapeOpacityProperty =
            DependencyProperty.RegisterAttached(nameof(ShapeOpacity),
                typeof(double),
                typeof(ColorPicker),
                new(1));

        public double ShapeOpacity
        {
            get => (double)GetValue(ShapeOpacityProperty);
            set => SetValue(ShapeOpacityProperty, value);
        }

        public static readonly DependencyProperty PathStrokeStartLineProperty =
            DependencyProperty.Register(
                nameof(PathStrokeStartLine),
                typeof(PenLineCap),
                typeof(ColorPicker),
                new(default));

        public PenLineCap PathStrokeStartLine
        {
            get => (PenLineCap)GetValue(PathStrokeStartLineProperty);
            set => SetValue (PathStrokeStartLineProperty, value);
        }

        public static readonly DependencyProperty PathStrokeEndLineProperty =
            DependencyProperty.Register(
                nameof(PathStrokeEndLine),
                typeof(PenLineCap),
                typeof(ColorPicker),
                new(default));

        public PenLineCap PathStrokeEndLine
        {
            get => (PenLineCap)GetValue(PathStrokeEndLineProperty);
            set => SetValue(PathStrokeEndLineProperty, value);
        }


        public ColorPicker()
        {
            this.InitializeComponent();
        }
    }
}
