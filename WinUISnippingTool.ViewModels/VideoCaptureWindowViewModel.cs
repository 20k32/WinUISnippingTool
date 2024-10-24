using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage;
using WinUISnippingTool.Helpers;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.VideoCapture;

namespace WinUISnippingTool.ViewModels;

public sealed class VideoCaptureWindowViewModel : ViewModelBase
{
    private readonly DispatcherTimer frameTimer;
    private Pen pen;
    private Graphics graphics;
    private nint deviceContextHandle;
    private readonly VideoCaptureHelper captureHelper;
    public MonitorLocation CurrentMonitor;
    public RectInt32 videoFrame;

    private string timeString = "00:00:00";

    public string TimeString
    {
        get => timeString;
        set
        {
            if(timeString != value)
            {
                timeString = value;
                NotifyOfPropertyChange();
            }
        }
    }

    private readonly Stopwatch stopwatch;
    private readonly DispatcherTimer videoTimer;

    public VideoCaptureWindowViewModel()
    {
        videoTimer = new();
        frameTimer = new();
        stopwatch = new();

        videoTimer.Interval = TimeSpan.FromSeconds(1);
        frameTimer.Interval = TimeSpan.FromMilliseconds(200);
        
        captureHelper = new();
        captureHelper
            .SetFramerate(CoreConstants.DefaultFramerate)
            .SetBitrate(CoreConstants.DefaultBitrate);
    }

    private void FrameTimerTick(object sender, object e)
    {
        graphics.DrawRectangle(pen, videoFrame.X, videoFrame.Y, videoFrame.Width, videoFrame.Height);
    }

    private void VideoTimerTick(object sender, object e)
    {
        TimeString = $"{stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}";
    }

    private void CreateContext()
    {
        deviceContextHandle = WindowExtensions.CreateDeviceContext("DISPLAY", CurrentMonitor.DeviceName);
        graphics = Graphics.FromHdc(deviceContextHandle);
        pen = new Pen(Brushes.Red);
    }

    private void DisposeContext()
    {
        WindowExtensions.ReleaseDeviceContext(deviceContextHandle);
        graphics.Dispose();
        pen.Dispose();
        pen = null;
    }

    public void StartTimers()
    {
        videoTimer.Tick += VideoTimerTick;
        frameTimer.Tick += FrameTimerTick;
        frameTimer.Start();
        videoTimer.Start();
        stopwatch.Start();
    }

    public void StopTimers()
    {
        videoTimer.Tick -= VideoTimerTick;
        frameTimer.Tick -= FrameTimerTick;
        frameTimer.Stop();
        videoTimer.Stop();
        stopwatch.Reset();
    }

    protected override void LoadLocalization(string bcpTag)
    {}

    public void SetCaptureSize(uint width, uint height)
    {
        captureHelper.SetSize(width, height);
    }

    public void SetMonitorForCapturing(MonitorLocation monitor) => CurrentMonitor = monitor;
    public void SetFrameForMonitor(RectInt32 frame) => videoFrame = frame;

    public Task StartCaptureAsync()
    {
        CreateContext();
        StartTimers();
        return captureHelper.StartScreenCaptureAsync(CurrentMonitor, videoFrame);
    }

    public void StopCapture()
    {
        StopTimers();
        DisposeContext();
        captureHelper.StopScreenCapture();
    }

    public Uri GetVideoUri() => captureHelper.CurrentVideoFileUri;
}
