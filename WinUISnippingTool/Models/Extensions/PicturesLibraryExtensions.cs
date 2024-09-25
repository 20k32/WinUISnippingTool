using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WinUISnippingTool.Models.Extensions
{
    internal static class PicturesLibraryExtensions
    {
        private static bool isBusy = false;
        public static async Task<StorageFile> SaveAsync(uint pixelWidth, uint pixelHeight, byte[] buffer)
        {
            var currDate = DateTime.Now;
            var fileName = $"Screenshot {currDate.ToString("yyyy-dd-MM")} {currDate.Hour}{currDate.Minute}{currDate.Second}.jpg";
            var screenshotsFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("Screenshots");

            var file = await screenshotsFolder.CreateFileAsync(fileName);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {

                var encoder = await BitmapEncoder.CreateAsync(BitmapSavingConstants.EncoderId, stream); // error here

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
