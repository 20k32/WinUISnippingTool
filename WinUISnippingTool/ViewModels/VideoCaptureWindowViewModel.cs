using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using WinUISnippingTool.Models;
using WinUISnippingTool.Models.VideoCapture;

namespace WinUISnippingTool.ViewModels;

internal class VideoCaptureWindowViewModel : ViewModelBase
{
    private VideoCaptureHelper captureHelper;
    private MonitorLocation currentMonitor;
    private RectInt32 videoFrame;

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
    private readonly DispatcherTimer timer;

    public VideoCaptureWindowViewModel()
    {
        timer = new();
        stopwatch = new();
        timer.Interval = TimeSpan.FromSeconds(1);
        
        captureHelper = new();
        captureHelper
            .SetFramerate(60)
            .SetBitrate(10_000_000);
    }

    private void Timer_Tick(object sender, object e)
    {
        TimeString = $"{stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}";
    }

    public void StartTimer()
    {
        timer.Tick += Timer_Tick;
        timer.Start();
        stopwatch.Start();
    }

    public void StopTimer()
    {
        timer.Tick -= Timer_Tick;
        timer.Stop();
        stopwatch.Reset();
    }

    protected override void LoadLocalization(string bcpTag)
    {}

    public void SetCaptureSize(uint width, uint height)
    {
        captureHelper.SetSize(width, height);
    }

    public void SetMonitorForCapturing(MonitorLocation monitor) => currentMonitor = monitor;
    public void SetFrameForMonitor(RectInt32 frame) => videoFrame = frame;

    public Task StartCaptureAsync()
    {
        StartTimer();
        return captureHelper.StartScreenCaptureAsync(currentMonitor, videoFrame);
    }

    public void StopCapture()
    {
        StopTimer();
        captureHelper.StopScreenCapture();
    }

    public Uri GetVideoUri() => captureHelper.CurrentVideoFileUri;
}
