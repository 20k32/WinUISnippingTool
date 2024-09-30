using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace WinUISnippingTool.Models.Extensions
{
    internal static class PicturesFolderExtensions
    {
        private static StorageFolder picturesLibraryScreenshotsFolder;
        public static StorageFolder NewSavingFolder;

        public static async Task<StorageFolder> GetScreenshotsFolderAsync()
        {
            return picturesLibraryScreenshotsFolder ??=
                await KnownFolders.PicturesLibrary.GetFolderAsync("Screenshots");
        }

        private static async Task<StorageFolder> DefineFolderAsync()
        {
            StorageFolder result;
            
            if(NewSavingFolder is null)
            {
                result = await GetScreenshotsFolderAsync();
            }
            else
            {
                result = NewSavingFolder;
            }

            return result;
        }

        public static async Task<StorageFile> SaveAsync(uint pixelWidth, uint pixelHeight, byte[] buffer)
        {
            var currDate = DateTime.Now;
            var fileName = $"Screenshot {currDate:yyyy-dd-MM} {currDate.Hour}{currDate.Minute}{currDate.Second}{BitmapSavingConstants.FileExtension}";
            var screenshotsFolder = await DefineFolderAsync();
            
            var file = await screenshotsFolder.CreateFileAsync(fileName);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {

                var encoder = await BitmapEncoder.CreateAsync(BitmapSavingConstants.EncoderId, stream);

                encoder.SetPixelData(
                     BitmapPixelFormat.Bgra8,
                     BitmapAlphaMode.Straight,
                     pixelWidth,
                     pixelHeight,
                     BitmapSavingConstants.DpiX,
                     BitmapSavingConstants.DpiY,
                     buffer);

                await encoder.FlushAsync();
            }

            return file;
        }
    }
}
