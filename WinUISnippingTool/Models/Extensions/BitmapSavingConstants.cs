using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace WinUISnippingTool.Models.Extensions;

internal static class BitmapSavingConstants
{
    public const int DpiX = 0;
    public const int DpiY = 0;
    public static readonly Guid EncoderId = BitmapEncoder.PngEncoderId;
    public const string FileExtension = ".png";
}
