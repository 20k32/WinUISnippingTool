using System;
using System.Threading;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using SharpDX.Direct3D11;
using WinUISnippingTool.Helpers.DirectX;
using Microsoft.UI.Xaml.Media;
using SharpDX.DXGI;
using SharpDX;
using System.Drawing.Imaging;
using Microsoft.UI.Xaml;
using Windows.Devices.HumanInterfaceDevice;
using Microsoft.Graphics.Canvas;
using System.Drawing;
using Windows.Win32;
using SharpDX.D3DCompiler;
using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Win32.Graphics.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using ABI.Windows.Foundation;
using Windows.Foundation;

namespace WinUISnippingTool.Models.VideoCapture;

internal sealed class CaptureFrameWait : IDisposable
{
    private IDirect3DDevice device;
    private SharpDX.Direct3D11.Device d3dDevice;
    private readonly SharpDX.Direct3D11.Multithread multithread;
    private Texture2D blankTexture;

    private readonly ManualResetEvent[] events;
    private readonly ManualResetEvent frameEvent;
    private readonly ManualResetEvent closedEvent;
    private Direct3D11CaptureFrame currentFrame;

    private GraphicsCaptureItem item;
    private GraphicsCaptureSession session;
    private Direct3D11CaptureFramePool framePool;

    private Windows.Foundation.Rect frameRect;
    private PointInt32 centerFrameCoords;

    public CaptureFrameWait(
        IDirect3DDevice device,
        GraphicsCaptureItem item,
        SizeInt32 size,
        RectInt32 frameRect,
        float pixelScaleX,
        float pixelScaleY)
    {
        this.device = device;
        d3dDevice = Direct3D11Helpers.CreateSharpDXDevice(device);

        multithread = d3dDevice.QueryInterface<SharpDX.Direct3D11.Multithread>();
        multithread.SetMultithreadProtected(true);
        this.item = item;
        frameEvent = new ManualResetEvent(false);
        closedEvent = new ManualResetEvent(false);
        events = new[] { closedEvent, frameEvent };

        InitializeBlankTexture(size);
        InitializeCapture(size);

        
        this.frameRect = new();

        this.frameRect.X = frameRect.X * pixelScaleX;
        this.frameRect.Width = frameRect.Width * pixelScaleX;

        this.frameRect.Y = frameRect.Y * pixelScaleY;
        this.frameRect.Height = frameRect.Height * pixelScaleY;

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
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription()
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
            d3dDevice.ImmediateContext.ClearRenderTargetView(renderTargetView, new RawColor4(0, 0, 0, 1));
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

        device?.Dispose();
        device = null;
        
        d3dDevice?.Dispose();
        d3dDevice = null;

        multithread?.Dispose();

        item = null;
        blankTexture?.Dispose();
        blankTexture = null;
        currentFrame?.Dispose();
        currentFrame = null;
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
                    Width = (int)frameRect.Width,
                    Height = (int)frameRect.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = sourceTexture.Description.Format,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                };

                using (var croppedTexture = new Texture2D(d3dDevice, croppedDescription))
                {
                    var region = new ResourceRegion(
                        (int)frameRect.X,
                        (int)frameRect.Y,
                        front: 0,
                        (int)(frameRect.X + frameRect.Width),
                        (int)(frameRect.Y + frameRect.Height),
                        back: 1);

                    d3dDevice.ImmediateContext.CopySubresourceRegion(sourceTexture, 0, region, croppedTexture, 0, 0, 0);

                    using (var scaledTexture = GetScaledTexture2D(currentFrame.ContentSize, croppedTexture))
                    {
                        var scaledSurface = Direct3D11Helpers.CreateDirect3DSurfaceFromSharpDXTexture(scaledTexture);
                        result.Surface = scaledSurface;
                    }
                }
            }
        }

        return result;
    }

    private Texture2D GetScaledTexture2D(SizeInt32 originalSize, Texture2D croppedTexture)
    {
        var resizedTextureDesc = new Texture2DDescription
        {
            Width = originalSize.Width,
            Height = originalSize.Height,
            ArraySize = 1,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            Usage = ResourceUsage.Default,
            CpuAccessFlags = CpuAccessFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            MipLevels = 1,
            OptionFlags = ResourceOptionFlags.None,
            SampleDescription = new SampleDescription(1, 0),
        };

        var resizedTexture = new Texture2D(d3dDevice, resizedTextureDesc);

        using (var factory = new SharpDX.Direct2D1.Factory())
        using (var surface = resizedTexture.QueryInterface<Surface>())
        {
            var renderProperties = new RenderTargetProperties(RenderTargetType.Default,
                   new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied), 0, 0, RenderTargetUsage.None, SharpDX.Direct2D1.FeatureLevel.Level_10);

            using (var renderTarget = new RenderTarget(factory, surface, renderProperties))
            {
                var bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));

                using (var smallerSurface = croppedTexture.QueryInterface<Surface>())
                using (var smallerBitmap = new SharpDX.Direct2D1.Bitmap(renderTarget, smallerSurface, bitmapProperties))
                {
                    float originalWidth = croppedTexture.Description.Width;
                    float originalHeight = croppedTexture.Description.Height;

                    var originalAspectRatio = originalWidth / originalHeight;
                    var targetAspectRatio = originalSize.Width / (float)originalSize.Height;

                    float scaledWidth, scaledHeight;

                    if (originalAspectRatio > targetAspectRatio)
                    {
                        scaledWidth = originalSize.Width;
                        scaledHeight = originalSize.Width / originalAspectRatio;
                    }
                    else
                    {
                        scaledHeight = originalSize.Height;
                        scaledWidth = originalSize.Height * originalAspectRatio;
                    }

                    var xOffset = (originalSize.Width - scaledWidth) / 2;
                    var yOffset = (originalSize.Height - scaledHeight) / 2;

                    RawRectangleF destinationRectangle = new RawRectangleF(xOffset, yOffset, xOffset + scaledWidth, yOffset + scaledHeight);
                    RawRectangleF sourceRectangle = new RawRectangleF(0, 0, originalWidth, originalHeight);

                    renderTarget.BeginDraw();
                    renderTarget.DrawBitmap(smallerBitmap, destinationRectangle, 1.0f, BitmapInterpolationMode.Linear, sourceRectangle);
                    renderTarget.EndDraw();
                }
            }
        }

        return resizedTexture;
    }

    public void Dispose()
    {
        Stop();
        Cleanup();
    }
}
