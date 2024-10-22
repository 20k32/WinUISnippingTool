using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Storage;
using WinUISnippingTool.Helpers;
using WinUISnippingTool.Helpers.DirectX;
using WinUISnippingTool.Helpers.Saving;
using WinUISnippingTool.Models.MonitorInfo;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading;

namespace WinUISnippingTool.Models.VideoCapture;

public sealed class VideoCaptureHelper
{
    public Uri CurrentVideoFileUri { get; private set; }

    private IDirect3DDevice device;
    private Encoder encoder;
    private VideoCaptureOptions options;

    public VideoCaptureHelper()
    {
        this.options = new();
    }

    public VideoCaptureHelper SetBitrate(uint bitrate)
    {
        options.Bitrate = bitrate;
        return this;
    } 
    public VideoCaptureHelper SetSize(uint width, uint height)
    {
        options.Width = width;
        options.Height = height;
        return this;
    }

    public VideoCaptureHelper SetFramerate(uint framrate)
    {
        options.Framerate = framrate;
        return this;
    }

    private static async Task<StorageFile> GetFileAsync()
    {
        var name = DateTime.Now.ToString("yyyyMMdd-HHmm-ss");
        var fileName = $"SnipT-{name}.mp4";
        var folder = await FolderExtensions.DefineFolderForVideosAsync();
        var file = await folder.CreateFileAsync(fileName);
        return file;
    }

    public async Task StartScreenCaptureAsync(MonitorLocation currentMonitor, RectInt32 videoFrameSize)
    {
        var graphicsCaptureItem = GraphicsCaptureItemExtensions.CreateItemForMonitor(currentMonitor.HandleMonitor);
        device = Direct3D11Helpers.CreateDevice();
        var file = await GetFileAsync();

        try
        {
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            using (encoder = new Encoder(device, graphicsCaptureItem, videoFrameSize, options))
            {
                await encoder.EncodeAsync(stream);
            }

            CurrentVideoFileUri = new($"file:///{file.Path}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);

            CurrentVideoFileUri = null;
        }
    }

    public void StopScreenCapture()
    {
        encoder.Dispose();
    }
}
