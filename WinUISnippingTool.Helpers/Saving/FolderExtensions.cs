using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WinUISnippingTool.Helpers.Saving;

public static class FolderExtensions
{
    public static StorageFolder NewPicturesSavingFolder;
    private static StorageFolder picturesLibraryFolder;

    public static StorageFolder NewVideosSavingFolder;
    private static StorageFolder videosLibraryFolder;

    public const string ScreenshotsFolderName = "Screenshots";
    public const string VideosFolderName = "Captures";

    public static async Task<StorageFolder> GetDefaultScreenshotsFolderAsync() =>
        picturesLibraryFolder ??= await KnownFolders.PicturesLibrary.GetFolderAsync(ScreenshotsFolderName);

    public static async Task<StorageFolder> GetDefaultVideosFolderAsync() =>
        videosLibraryFolder ??= await KnownFolders.VideosLibrary.GetFolderAsync(VideosFolderName);

    private static async Task<StorageFolder> DefineFolderForPicturesAsync()
    {
        StorageFolder result;

        if (NewPicturesSavingFolder is null)
        {
            result = await GetDefaultScreenshotsFolderAsync();
        }
        else
        {
            result = NewPicturesSavingFolder;
        }

        return result;
    }

    public static async Task<StorageFolder> DefineFolderForVideosAsync()
    {
        StorageFolder result;

        if (NewVideosSavingFolder is null)
        {
            result = await GetDefaultVideosFolderAsync();
        }
        else
        {
            result = NewVideosSavingFolder;
        }

        return result;
    }

    public static async Task<StorageFile> SaveBitmapAsync(SoftwareBitmap softwareBitmap)
    {
        var currDate = DateTime.Now;
        var fileName = $"Screenshot {currDate:yyyy-dd-MM} {currDate.Hour}{currDate.Minute}{currDate.Second}{BitmapSavingConstants.FileExtension}";
        var screenshotsFolder = await DefineFolderForPicturesAsync();

        var file = await screenshotsFolder.CreateFileAsync(fileName);

        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        {

            var encoder = await BitmapEncoder.CreateAsync(BitmapSavingConstants.EncoderId, stream);

            encoder.SetSoftwareBitmap(softwareBitmap);

            await encoder.FlushAsync();
        }

        return file;
    }
}
