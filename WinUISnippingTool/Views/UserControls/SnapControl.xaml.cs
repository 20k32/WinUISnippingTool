using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUISnippingTool.Models.Items;
using WinUISnippingTool.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.UserControls
{
    internal sealed partial class SnapControl : UserControl
    {
        public string Title => "Hell to world";
        public SnapControl()
        {
            this.InitializeComponent();
        }

        public readonly static DependencyProperty SnipKindsProperty = DependencyProperty.Register(
            nameof(SnipKinds),
            typeof(NotifyOnCompleteAddingCollection<SnipShapeKind>),
            typeof(SnapControl), new(default));

        public NotifyOnCompleteAddingCollection<SnipShapeKind> SnipKinds
        {
            get => (NotifyOnCompleteAddingCollection<SnipShapeKind>)GetValue(SnipKindsProperty);
            set => SetValue(SnipKindsProperty, value);
        }

        public static DependencyProperty SelectedSnipKindProperty = DependencyProperty.Register(
            nameof(SelectedSnipKind),
            typeof(SnipShapeKind),
            typeof(SnapControl),
            new(default));

        private SnipShapeKind selectedSnipKind;

        public SnipShapeKind SelectedSnipKind
        {
            get => (SnipShapeKind)GetValue(SelectedSnipKindProperty);
            set => SetValue(SelectedSnipKindProperty, value);
        }
    }
}
