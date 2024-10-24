﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage.Streams;
using Windows.Graphics;

namespace WinUISnippingTool.Models.VideoCapture;

internal sealed class Encoder : IDisposable
{
    private readonly IDirect3DDevice device;

    private readonly GraphicsCaptureItem captureItem;
    private CaptureFrameWait frameGenerator;
    private RectInt32 frameSize;
    private VideoCaptureOptions options;
    private VideoStreamDescriptor videoDescriptor;
    private MediaStreamSource mediaStreamSource;
    private MediaTranscoder transcoder;
    private bool isRecording;
    private bool closed = false;

    public Encoder(IDirect3DDevice device, GraphicsCaptureItem item, RectInt32 frameSize, VideoCaptureOptions options)
    {
        
        this.device = device;
        captureItem = item;
        isRecording = false;
        this.frameSize = frameSize;
        this.options = options;

        CreateMediaObjects();
    }

    public IAsyncAction EncodeAsync(IRandomAccessStream stream)
    {
        return EncodeInternalAsync(stream).AsAsyncAction();
    }

    private async Task EncodeInternalAsync(IRandomAccessStream stream)
    {
        if (!isRecording)
        {
            isRecording = true;
           
            frameGenerator = new CaptureFrameWait(
                device,
                captureItem,
                captureItem.Size,
                frameSize,
                options.DpiDependentPixelScaleX,
                options.DpiDependentPixelScaleY);

            using (frameGenerator)
            {
                var encodingProfile = new MediaEncodingProfile();
                encodingProfile.Container.Subtype = "MPEG4";
                encodingProfile.Video.Subtype = "H264";
                encodingProfile.Video.Width = options.Width;
                encodingProfile.Video.Height = options.Height;
                encodingProfile.Video.Bitrate = options.Bitrate;
                encodingProfile.Video.FrameRate.Numerator = options.Framerate;
                encodingProfile.Video.FrameRate.Denominator = 1;
                encodingProfile.Video.PixelAspectRatio.Numerator = 1;
                encodingProfile.Video.PixelAspectRatio.Denominator = 1;
                var transcode = await transcoder.PrepareMediaStreamSourceTranscodeAsync(mediaStreamSource, stream, encodingProfile);
                
                try
                {
                    await transcode.TranscodeAsync(); 
                }
                catch(Exception ex)
                {
                    Debug.Write(ex.Message);
                }
            }
        }
    }

    public void Dispose()
    {
        if (!closed)
        {
            closed = true;

            if (!isRecording)
            {
                DisposeInternal();
            }

            isRecording = false;
        }
    }

    private void DisposeInternal()
    {
        frameGenerator.Dispose();
    }

    private void CreateMediaObjects()
    {
        int width = captureItem.Size.Width;
        int height = captureItem.Size.Height;

        var videoProperties = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Bgra8, (uint)width, (uint)height);
        videoDescriptor = new VideoStreamDescriptor(videoProperties);

        mediaStreamSource = new MediaStreamSource(videoDescriptor);
        mediaStreamSource.BufferTime = TimeSpan.FromSeconds(0);
        mediaStreamSource.Starting += OnMediaStreamSourceStarting;
        mediaStreamSource.SampleRequested += OnMediaStreamSourceSampleRequested;
        
        transcoder = new MediaTranscoder();
        transcoder.HardwareAccelerationEnabled = true;
    }

    private void OnMediaStreamSourceSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
    {
        if (isRecording && !closed)
        {
            try
            {
                using (var frame = frameGenerator.WaitForNewFrame())
                {
                    if (frame == null)
                    {
                        args.Request.Sample = null;
                        DisposeInternal();
                    }
                    else
                    {
                        var timeStamp = frame.SystemRelativeTime;

                        var sample = MediaStreamSample.CreateFromDirect3D11Surface(frame.Surface, timeStamp);
                        args.Request.Sample = sample;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                Debug.WriteLine(e);
                args.Request.Sample = null;
                DisposeInternal();
            }
        }
        else
        {
            args.Request.Sample = null;
            DisposeInternal();
        }
    }

    private void OnMediaStreamSourceStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
    {
        using (var frame = frameGenerator.WaitForNewFrame())
        {
            args.Request.SetActualStartPosition(frame.SystemRelativeTime);
        }
    }
}
