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
    private static readonly FileSavePicker videoSavePicker;
    private static readonly FileSavePicker imageSavePicker;
    private static readonly FolderPicker folderPicker;

    static FilePickerExtensions()
    {
        videoSavePicker = new FileSavePicker()
        {
            DefaultFileExtension = ".mp4",
            SuggestedFileName = "Video",
            SuggestedStartLocation = PickerLocationId.Desktop,
            CommitButtonText = "Save",
        };

        imageSavePicker = new FileSavePicker()
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

        imageSavePicker.FileTypeChoices.Add(".png", new[] { ".png" } );
        videoSavePicker.FileTypeChoices.Add(".mp4", new[] { ".mp4" });
        folderPicker.FileTypeFilter.Add(".png");
    }

    public static void SetWindowHandle(nint windowHandle)
    {
        InitializeWithWindow.Initialize(videoSavePicker, windowHandle);
        InitializeWithWindow.Initialize(imageSavePicker, windowHandle);
        InitializeWithWindow.Initialize(folderPicker, windowHandle);
    }

    public static IAsyncOperation<StorageFile> ShowSaveVideoAsync()
        => videoSavePicker.PickSaveFileAsync();

    public static IAsyncOperation<StorageFile> ShowSaveImageAsync() 
        => imageSavePicker.PickSaveFileAsync();

    public static IAsyncOperation<StorageFolder> ShowFolderPickerAsync()
        => folderPicker.PickSingleFolderAsync();
}