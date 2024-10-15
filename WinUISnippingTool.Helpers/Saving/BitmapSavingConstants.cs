using System;
using Windows.Graphics.Imaging;

namespace WinUISnippingTool.Helpers.Saving;

public static class BitmapSavingConstants
{
    public const int Dpi = 96;
    public static readonly Guid EncoderId = BitmapEncoder.PngEncoderId;
    public const string FileExtension = ".png";
}
