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

namespace WinUISnippingTool.SharedResources.Controls;

public sealed partial class SnipControl : UserControl, INotifyPropertyChanged
{
    private static SnipShapeKind tempSelectedSnipKind;

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

    private static bool isDirectXSupported;
    public bool IsDirectXSupported
    {
        get => isDirectXSupported;
        set
        {
            if(isDirectXSupported != value)
            {
                isDirectXSupported = value;
                OnPropertyChanged();
                VideoButtonClickCommand.NotifyCanExecuteChanged();
            }
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

    private static void ChangeButtonsStateStatic(bool isPhotoMode)
    {
        isPhotoButtonEnabled = isPhotoMode;
        isVideoButtonEnabled = !isPhotoMode;
        isPaintListEnabled = !isPhotoMode;
    }

    private void ChangeButtonsState(bool isPhotoMode)
    {
        IsPhotoButtonEnabled = isPhotoMode;
        IsVideoButtonEnabled = !isPhotoMode;
        IsPaintListEnabled = !isPhotoMode;
    }


    [RelayCommand]
    private void PhotoButtonClick()
    {
        SelectedCaptureType = CaptureType.Photo;
        ChangeButtonsState(false);
    }


    [RelayCommand(CanExecute = nameof(CanVideoButtonClick))]
    private void VideoButtonClick()
    {
        SelectedCaptureType = CaptureType.Video;
        ChangeButtonsState(true);
    }

    private bool CanVideoButtonClick() => IsDirectXSupported; 


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

    public static readonly DependencyProperty SelectedCaptureTypeProperty = DependencyProperty.Register(
        nameof(SelectedCaptureType),
        typeof(CaptureType),
        typeof(SnipControl),
        new(default));

    private static void SelectedCaptureTypePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var newCaptureType = (CaptureType)e.NewValue;

        if(newCaptureType == CaptureType.Photo)
        {
            ChangeButtonsStateStatic(isPhotoMode: false);
        }
        else
        {
            ChangeButtonsStateStatic(isPhotoMode: true);
        }
    }

    public CaptureType SelectedCaptureType
    {
        get => (CaptureType)GetValue(SelectedCaptureTypeProperty);
        set => SetValue(SelectedCaptureTypeProperty, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
