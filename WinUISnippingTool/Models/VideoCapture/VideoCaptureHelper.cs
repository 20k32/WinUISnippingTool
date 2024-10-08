using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Storage;
using Windows.System;

namespace WinUISnippingTool.Models.VideoCapture;

internal class VideoCaptureHelper
{
    public Uri CurrentVideoFileUri { get; private set; }

    private IDirect3DDevice device;
    private Encoder encoder;
    private VideoCaptureOptions options;

    public void SetOptions(VideoCaptureOptions options) 
        => this.options = options;

    private static async Task<StorageFile> GetFile()
    {
        var name = DateTime.Now.ToString("yyyyMMdd-HHmm-ss");
        var fileName = $"{name}.mp4";

        var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName);
        return file;
    }

    public async Task StartScreenCaptureAsync(MonitorLocation currentMonitor, RectInt32 videoFrameSize)
    {
        var graphicsCaptureItem = GraphicsCaptureItemExtensions.CreateItemForMonitor(currentMonitor.HandleMonitor);
        device = Direct3D11Helpers.CreateDevice();
        var currentFile = await GetFile();

        try
        {
            using (var stream = await currentFile.OpenAsync(FileAccessMode.ReadWrite))
            using (encoder = new Encoder(device, graphicsCaptureItem, videoFrameSize, options))
            {
                await encoder.EncodeAsync(
                    stream,
                    (uint)currentMonitor.MonitorSize.Width, 
                    (uint)currentMonitor.MonitorSize.Height,
                    options.Bitrate,
                    options.Framerate);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex);
        }

        CurrentVideoFileUri = new($"file:///{currentFile.Path}");
    }

    public void StopScreenCapture()
    {
        encoder.Dispose();
    }
}
