using System;
using System.Threading;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using SharpDX.Direct3D11;

namespace WinUISnippingTool.Models.VideoCapture;

internal sealed class CaptureFrameWait : IDisposable
{
    private IDirect3DDevice device;
    private Device d3dDevice;
    private readonly Multithread multithread;
    private Texture2D blankTexture;

    private readonly ManualResetEvent[] events;
    private readonly ManualResetEvent frameEvent;
    private readonly ManualResetEvent closedEvent;
    private Direct3D11CaptureFrame currentFrame;

    private GraphicsCaptureItem item;
    private GraphicsCaptureSession session;
    private Direct3D11CaptureFramePool framePool;

    private RectInt32 frameRect;
    private PointInt32 centerFrameCoords;

    public CaptureFrameWait(
        IDirect3DDevice device,
        GraphicsCaptureItem item,
        SizeInt32 size,
        RectInt32 frameRect)
    {
        this.device = device;
        d3dDevice = Direct3D11Helpers.CreateSharpDXDevice(device);
        
        multithread = d3dDevice.QueryInterface<Multithread>();
        multithread.SetMultithreadProtected(true);
        this.item = item;
        frameEvent = new ManualResetEvent(false);
        closedEvent = new ManualResetEvent(false);
        events = new[] { closedEvent, frameEvent };

        InitializeBlankTexture(size);
        InitializeCapture(size);

        this.frameRect = frameRect;
        centerFrameCoords = new PointInt32();
        centerFrameCoords.X = (size.Width - frameRect.Width) / 2;
        centerFrameCoords.Y = (size.Height - frameRect.Height) / 2;
    }

    private void InitializeCapture(SizeInt32 size)
    {
        item.Closed += OnClosed;
        framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
            device,
            DirectXPixelFormat.B8G8R8A8UIntNormalized,
            1,
            size);
        framePool.FrameArrived += OnFrameArrived;
        session = framePool.CreateCaptureSession(item);
        session.StartCapture();
    }

    private void InitializeBlankTexture(SizeInt32 size)
    {
        var description = new Texture2DDescription
        {
            Width = size.Width,
            Height = size.Height,
            MipLevels = 1,
            ArraySize = 1,
            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
            SampleDescription = new SharpDX.DXGI.SampleDescription()
            {
                Count = 1,
                Quality = 0
            },
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None
        };

        blankTexture = new Texture2D(d3dDevice, description);
        
        using (var renderTargetView = new RenderTargetView(d3dDevice, blankTexture))
        {
            d3dDevice.ImmediateContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1));
        }
    }

    private void SetResult(Direct3D11CaptureFrame frame)
    {
        currentFrame = frame;
        frameEvent.Set();
    }

    private void Stop()
    {
        closedEvent.Set();
    }

    private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
    {
        SetResult(sender.TryGetNextFrame());
    }

    private void OnClosed(GraphicsCaptureItem sender, object args)
    {
        Stop();
    }

    private void Cleanup()
    {
        framePool?.Dispose();
        session?.Dispose();
        if (item != null)
        {
            item.Closed -= OnClosed;
        }
        item = null;
        device = null;
        d3dDevice = null;
        blankTexture?.Dispose();
        blankTexture = null;
        currentFrame?.Dispose();
    }

    public SurfaceWithInfo WaitForNewFrame()
    {
        currentFrame?.Dispose();
        frameEvent.Reset();
        SurfaceWithInfo result = null;

        var signaledEvent = events[WaitHandle.WaitAny(events)];

        if (signaledEvent == closedEvent)
        {
            Cleanup();
        }
        else
        {
            result = new SurfaceWithInfo();
            result.SystemRelativeTime = currentFrame.SystemRelativeTime;

            using (var multithreadLock = new MultithreadLock(multithread))
            using (var sourceTexture = Direct3D11Helpers.CreateSharpDXTexture2D(currentFrame.Surface))
            {
                var croppedDescription = new Texture2DDescription
                {
                    Width = currentFrame.Surface.Description.Width,
                    Height = currentFrame.Surface.Description.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = sourceTexture.Description.Format,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                };

                using (var croppedTexture = new Texture2D(d3dDevice, croppedDescription))
                {

                    var region = new ResourceRegion(
                        frameRect.X,
                        frameRect.Y,
                        0,
                        frameRect.Width + frameRect.X,
                        frameRect.Height + frameRect.Y,
                        1);

                    d3dDevice.ImmediateContext.CopyResource(blankTexture, croppedTexture);
                    d3dDevice.ImmediateContext.CopySubresourceRegion(sourceTexture, 0, region, croppedTexture, 0, centerFrameCoords.X, centerFrameCoords.Y);

                    // Create the surface from the cropped texture
                    result.Surface = Direct3D11Helpers.CreateDirect3DSurfaceFromSharpDXTexture(croppedTexture);
                }
            }
        }

        return result;
    }


    public void Dispose()
    {
        Stop();
        Cleanup();
    }
}
