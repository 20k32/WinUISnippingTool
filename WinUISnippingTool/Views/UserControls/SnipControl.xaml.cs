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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views.UserControls
{
    internal sealed partial class SnipControl : UserControl, INotifyPropertyChanged
    {
        private static SnipShapeKind tempSelectedSnipKind;

        public static CaptureType CaptureKind;

        static SnipControl()
        { }

        public SnipControl()
        {
            this.InitializeComponent();
        }

        private static bool isPhotoButtonEnabled;

        public bool IsPhotoButtonEnabled
        {
            get => isPhotoButtonEnabled;
            set
            {
                isPhotoButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        private static bool isVideoButtonEnabled;

        public bool IsVideoButtonEnabled
        {
            get => isVideoButtonEnabled;
            set
            {
                isVideoButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        private static bool isPaintListEnabled;

        public bool IsPaintListEnabled
        {
            get => isPaintListEnabled;
            set
            {
                isPaintListEnabled = value;

                if (isPaintListEnabled)
                {
                    if (tempSelectedSnipKind is not null)
                    {
                        SelectedSnipKind = tempSelectedSnipKind;
                    }
                }
                else
                {
                    tempSelectedSnipKind = SelectedSnipKind;
                    SelectedSnipKind = SnipKinds.First();
                }

                OnPropertyChanged();
            }
        }


        [RelayCommand]
        private void PhotoButtonClick()
        {
            CaptureKind = CaptureType.Photo;
            IsPhotoButtonEnabled = false;
            IsVideoButtonEnabled = true;
            IsPaintListEnabled = true;
        }


        [RelayCommand(CanExecute = nameof(CanVideoButtonClick))]
        private void VideoButtonClick()
        {
            CaptureKind = CaptureType.Video;
            IsPhotoButtonEnabled = true;
            IsVideoButtonEnabled = false;
            IsPaintListEnabled = false;
        }

        private bool CanVideoButtonClick() => App.IsDirectXSupported; 


        public readonly static DependencyProperty SnipKindsProperty = DependencyProperty.Register(
            nameof(SnipKinds),
            typeof(NotifyOnCompletionCollection<SnipShapeKind>),
            typeof(SnipControl), new(default));

        public NotifyOnCompletionCollection<SnipShapeKind> SnipKinds
        {
            get => (NotifyOnCompletionCollection<SnipShapeKind>)GetValue(SnipKindsProperty);
            set => SetValue(SnipKindsProperty, value);
        }

        public static readonly DependencyProperty SelectedSnipKindProperty = DependencyProperty.Register(
            nameof(SelectedSnipKind),
            typeof(SnipShapeKind),
            typeof(SnipControl),
            new(default));

        public SnipShapeKind SelectedSnipKind
        {
            get => (SnipShapeKind)GetValue(SelectedSnipKindProperty);
            set => SetValue(SelectedSnipKindProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;



        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
