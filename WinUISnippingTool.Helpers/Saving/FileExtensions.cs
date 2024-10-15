using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace WinUISnippingTool.Helpers.Saving;

public static class FileExtensions
{
    public static async Task SaveBmpBufferAsync(StorageFile file, uint pixelWidth, uint pixelHeight, byte[] buffer)
    {
        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        {

            var encoder = await BitmapEncoder.CreateAsync(BitmapSavingConstants.EncoderId, stream);

            encoder.SetPixelData(
                 BitmapPixelFormat.Bgra8,
                 BitmapAlphaMode.Premultiplied,
                 pixelWidth,
                 pixelHeight,
                 BitmapSavingConstants.Dpi,
                 BitmapSavingConstants.Dpi,
                 buffer);

            await encoder.FlushAsync();
        }
    }
}
