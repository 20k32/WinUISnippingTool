using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using WinUISnippingTool.Helpers;
using WinUISnippingTool.Models;

namespace WinUISnippingTool.Core;

public sealed class WindowSizeManager
{
    public double NewWidth { get; private set; }
    public double NewHeight { get; private set; }

    public bool IsSmall { get; private set; }
    public bool IsMiddle { get; private set; }
    public bool IsLarge { get; private set; }

    public event Action<bool> OnSmallSizeRequested;
    public event Action<bool> OnMiddleSizeRequested;
    public event Action<bool> OnLargeSizeRequested;

    private Action callback;

    private readonly DispatcherTimer timer;

    public WindowSizeManager(Action timerCallback)
    {
        callback = timerCallback;

        timer = new()
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };
    }

    public void RegisterHandlers()
    {
        timer.Start();
        timer.Tick += Timer_Tick;
    }

    public void UnregisterHandlers()
    {
        timer.Stop();
        timer.Tick -= Timer_Tick;
    }

    private void Timer_Tick(object sender, object e)
    {
        callback();
    }

    public void OnSizeChanged(Size newSize)
    {
        NewWidth = newSize.Width;
        NewHeight = newSize.Height;

        if (IsMiddle)
        {
            NewHeight -= CoreConstants.BottomPanelHeight;
        }

        if (!IsLarge
             && newSize.Width < CoreConstants.MaxLargeWidth)
        {
            OnLargeSizeRequested?.Invoke(IsLarge);
            IsLarge = true;
        }
        else if (!IsMiddle
                 && newSize.Width < CoreConstants.MaxMediumWidth)
        {
            OnMiddleSizeRequested?.Invoke(IsMiddle);
            IsMiddle = true;
        }
        else if (!IsSmall
                && newSize.Width < CoreConstants.MaxSmallWidth)
        {
            OnSmallSizeRequested?.Invoke(IsSmall);
            IsSmall = true;
        }
        else
        {
            if (IsLarge
                     && newSize.Width > CoreConstants.MaxLargeWidth)
            {
                OnLargeSizeRequested?.Invoke(IsLarge);
                IsLarge = false;
            }
            else if (IsMiddle
                && newSize.Width > CoreConstants.MaxMediumWidth)
            {
                OnMiddleSizeRequested?.Invoke(IsMiddle);
                IsMiddle = false;
            }
            else if (IsSmall
                && newSize.Width > CoreConstants.MaxSmallWidth)
            {
                OnSmallSizeRequested?.Invoke(IsSmall);
                IsSmall = false;
            }
        }
    }
}
