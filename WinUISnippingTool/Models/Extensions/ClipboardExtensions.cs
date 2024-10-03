using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace WinUISnippingTool.Models.Extensions
{
    internal static class ClipboardExtensions
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
                    BitmapSavingConstants.DpiX,
                    BitmapSavingConstants.DpiY,
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
                
                try
                {
                    Clipboard.SetContent(dataPackage);
                    Clipboard.Flush();
                }
                catch (Exception e)
                {
                    var ex = e;
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
