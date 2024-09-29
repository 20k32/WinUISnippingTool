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
    private static readonly FolderPicker folderPicker;

    static FilePickerExtensions()
    {
        fileSavePicker = new FileSavePicker()
        {
            DefaultFileExtension = ".jpeg",
            SuggestedFileName = "Image",
            SuggestedStartLocation = PickerLocationId.Desktop,
            CommitButtonText = "Save",
        };

        folderPicker = new FolderPicker()
        {
            CommitButtonText = "Ok",
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.Desktop,
        };

        fileSavePicker.FileTypeChoices.Add(".png", new[] { ".png" } );
        folderPicker.FileTypeFilter.Add(".png");
    }

    public static void SetWindowHandle(nint windowHandle)
    {
        InitializeWithWindow.Initialize(fileSavePicker, windowHandle);
        InitializeWithWindow.Initialize(folderPicker, windowHandle);
    }

    public static IAsyncOperation<StorageFile> ShowSaveAsync() 
        => fileSavePicker.PickSaveFileAsync();

    public static IAsyncOperation<StorageFolder> ShowFolderPickerAsync()
        => folderPicker.PickSingleFolderAsync();
}