﻿using System;
using Windows.Graphics.DirectX.Direct3D11;

namespace WinUISnippingTool.Models.VideoCapture;

internal sealed class SurfaceWithInfo : IDisposable
{
    public IDirect3DSurface Surface { get; internal set; }
    public TimeSpan SystemRelativeTime { get; internal set; }

    public void Dispose()
    {
        Surface?.Dispose();
        Surface = null;
    }
}
