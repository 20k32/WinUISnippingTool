using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace WinUISnippingTool.Helpers.Saving;

public static class ClipboardExtensions
{
    public static async Task CopyAsync(uint pixelWidth, uint pixelHeight, byte[] buffer)
    {
        using (var stream = new InMemoryRandomAccessStream())
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

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }

    public static async Task CopyAsync(SoftwareBitmap softwareBitmap)
    {
        using (var stream = new InMemoryRandomAccessStream())
        {
            var encoder = await BitmapEncoder.CreateAsync(BitmapSavingConstants.EncoderId, stream);

            encoder.SetSoftwareBitmap(softwareBitmap);
            await encoder.FlushAsync();

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            dataPackage.RequestedOperation = DataPackageOperation.Copy;

            var options = new ClipboardContentOptions()
            {
                IsAllowedInHistory = true,
                IsRoamable = false
            };

            Clipboard.SetContentWithOptions(dataPackage, options);

            try
            {
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
