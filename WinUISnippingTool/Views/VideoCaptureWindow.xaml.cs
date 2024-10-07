using Microsoft.UI.Xaml;
using System;
using System.IO;
using Windows.Graphics;
using WinUISnippingTool.Models;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Microsoft.Graphics.Canvas;
using Windows.Storage;
using WinRT.Interop;
using WinUISnippingTool.Models.VideoCapture;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using Windows.Media.MediaProperties;
using Windows.Media;
using Windows.Media.Editing;
using System.Threading.Tasks;
using Windows.Media.Core;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using Windows.Media.Transcoding;
using SharpDX.Multimedia;
using Windows.UI.Popups;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUISnippingTool.Views;

internal sealed partial class VideoCaptureWindow : Window
{

    private nint handle;
    private MediaOutput file;
    private Direct3D11CaptureFramePool framePool;
    private GraphicsCaptureSession session;
    private GraphicsCapturePicker picker;
    private CanvasDevice canvasDevice;
    private System.Drawing.Size VideoSize;
    private MediaComposition mediaComposition;
    private StorageFile currentFile;
    private MediaEncodingProfile _encodingProfile;
    private Stopwatch stopwatch;
    private Encoder encoder;
    IDirect3DDevice _device;
    
    public VideoCaptureWindow()
    {
        this.InitializeComponent();
        canvasDevice = CanvasDevice.GetSharedDevice();
        mediaComposition = new();
        stopwatch = new();
    }

    public void PrepareWindow(RectInt32 windowLocation)
    {

        //AppWindow.MoveAndResize(windowLocation);

        /* var presenter = ((OverlappedPresenter)AppWindow.Presenter);
         presenter.IsMinimizable = false;
         presenter.IsMaximizable = false;
         presenter.IsResizable = false;
         AppWindow.IsShownInSwitchers = false;
         presenter.SetBorderAndTitleBar(false, false);*/

        picker = new GraphicsCapturePicker();
        handle = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(picker, handle);
    }

    private async Task<StorageFile> GetTempFilePath()
    {
        var folder = ApplicationData.Current.TemporaryFolder;
        var name = DateTime.Now.ToString("yyyyMMdd-HHmm-ss");
        var fileName = $"{name}.mp4";
        //var path = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, fileName);
        
        var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName);
        return file;
    }

    public async void StartScreenCapture(MonitorLocation monitor)
    {
        var graphicsCaptureItem = GraphicsCaptureItemExtensions.CreateItemForMonitor(monitor.HandleMonitor);
        _device = Direct3D11Helpers.CreateDevice();

        VideoSize = new(500, 500);
        currentFile = await GetTempFilePath();
        var temp = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

        uint width = 500;
        uint height = 500;
        uint bitrate = 9000000;
        uint frameRate = 60;
   

        // Kick off the encoding
        try
        {
            using (var stream = await currentFile.OpenAsync(FileAccessMode.ReadWrite))
            using (encoder = new Encoder(_device, graphicsCaptureItem))
            {
                //var surface = encoder.CreatePreviewSurface(Compositor);
                //encoder.Surface = surface;

                await encoder.EncodeAsync(
                    stream,
                    1920, 1080, 
                    bitrate,
                    frameRate);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex);
        }
        
        await Launcher.LaunchFileAsync(currentFile);

        /*_encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
        _encodingProfile.Video.Height = (uint)VideoSize.Height;
        _encodingProfile.Video.Width = (uint)VideoSize.Width;

        var encodingOptions = new VideoEncodingProperties()
        {
            Bitrate = 18000000,
            Height = (uint)VideoSize.Height,
            Width = (uint)VideoSize.Width
        };

        var descriptor = new VideoStreamDescriptor(encodingOptions);
        _encodingProfile.SetVideoTracks(new[] {descriptor});*/

        /* framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                 _device,
                 Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                 1,
                 graphicsCaptureItem.Size);

         session = framePool.CreateCaptureSession(graphicsCaptureItem);
         framePool.FrameArrived += FramePool_FrameArrived;


         session.StartCapture();

         stopwatch.Start();*/
    }

    TimeSpan frameTime = TimeSpan.FromMilliseconds(1 / 60d);
    TimeSpan startTime = TimeSpan.FromMilliseconds(16);
    TimeSpan prevTime = TimeSpan.Zero;

    private void FramePool_FrameArrived(Direct3D11CaptureFramePool sender, object args)
    {
        using (var frame = sender.TryGetNextFrame())
        {
            if (stopwatch.Elapsed >= frameTime)
            {
                startTime += prevTime;

                var clip = MediaClip.CreateFromSurface(frame.Surface, frameTime);
                mediaComposition.Clips.Add(clip);

                prevTime = startTime;

                stopwatch.Restart();
            }            
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        /*framePool.Dispose();
        session.Dispose();

        var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
        encodingProfile.Video.Bitrate = 10000000;
        encodingProfile.Video.FrameRate.Numerator = 60;
        encodingProfile.Video.FrameRate.Denominator = 1;
       
        await mediaComposition.RenderToFileAsync(currentFile, MediaTrimmingPreference.Fast);*/

        encoder?.Dispose();
    }
}
