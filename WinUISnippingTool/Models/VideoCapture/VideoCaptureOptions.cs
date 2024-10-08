namespace WinUISnippingTool.Models.VideoCapture;

internal struct VideoCaptureOptions
{
    public uint Width;
    public uint Height;
    public uint Bitrate;
    public uint Framerate;

    public VideoCaptureOptions(uint width, uint height, uint bitrate, uint framerate)
        => (Width, Height, Bitrate, Framerate) = (width, height, bitrate, framerate);
}
