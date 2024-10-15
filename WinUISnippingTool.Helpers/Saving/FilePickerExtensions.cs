using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace WinUISnippingTool.Helpers.Saving;

public static class FilePickerExtensions
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

        imageSavePicker.FileTypeChoices.Add(".png", [".png"]);
        videoSavePicker.FileTypeChoices.Add(".mp4", [".mp4"]);
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