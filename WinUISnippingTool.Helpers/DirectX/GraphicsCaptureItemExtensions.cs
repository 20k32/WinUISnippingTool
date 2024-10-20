﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.WinRT.Graphics.Capture;
using Windows.Win32.Foundation;


namespace WinUISnippingTool.Helpers.DirectX;

public static class GraphicsCaptureItemExtensions
{
    public static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    public static GraphicsCaptureItem CreateItemForWindow(HWND hwnd)
    {
        GraphicsCaptureItem item = null;

        unsafe
        {
            item = CreateItemForCallback((interop, guid) =>
            {
                interop.CreateForWindow(hwnd, guid, out object raw);
                return raw;
            });
        }
        return item;
    }

    public static GraphicsCaptureItem CreateItemForMonitor(HMONITOR hmon)
    {
        GraphicsCaptureItem item = null;
        unsafe
        {
            item = CreateItemForCallback((interop, guid) =>
            {
                interop.CreateForMonitor(hmon, guid, out object raw);
                return raw;
            });
        }
        return item;
    }

    private unsafe delegate object InteropCallback(IGraphicsCaptureItemInterop interop, Guid* guid);

    private static GraphicsCaptureItem CreateItemForCallback(InteropCallback callback)
    {
        var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
        GraphicsCaptureItem item = null;
        unsafe
        {
            var guid = GraphicsCaptureItemGuid;
            var guidPointer = (Guid*)Unsafe.AsPointer(ref guid);
            var raw = Marshal.GetIUnknownForObject(callback(interop, guidPointer));
            item = GraphicsCaptureItem.FromAbi(raw);
            Marshal.Release(raw);
        }
        return item;
    }
}
