using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Runtime.CompilerServices;
using WinRT.Interop;

namespace WinUISnippingTool.Models.Extensions;

internal static class FilePickerExtensions
{
    private static readonly FileSavePicker fileSavePicker;

    static FilePickerExtensions()
    {
        fileSavePicker = new FileSavePicker()
        {
            DefaultFileExtension = ".jpeg",
            SuggestedFileName = "Image",
            SuggestedStartLocation = PickerLocationId.Desktop,
            CommitButtonText = "Save",
        };

        fileSavePicker.FileTypeChoices.Add(".jpeg", new[] { ".jpeg" } );
    }

    public static void SetWindowHandle(nint windowHandle)
    {
        InitializeWithWindow.Initialize(fileSavePicker, windowHandle);
    }

    public static IAsyncOperation<StorageFile> ShowSaveAsync() 
        => fileSavePicker.PickSaveFileAsync();
}