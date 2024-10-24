﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;
using Windows.Win32.Graphics.Direct3D11;
using static Windows.Win32.PInvoke;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Graphics.Direct3D;
using SharpDX.Direct3D11;


namespace WinUISnippingTool.Helpers.DirectX;

[ComImport]
[Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComVisible(true)]
public interface IDirect3DDxgiInterfaceAccess
{
    nint GetInterface([In] ref Guid iid);
};

public static class Direct3D11Helpers
{
    internal static Guid ID2D1Factory = new("06152291-69C8-4675-9F29-9A33B4A7B1D6");
    internal static Guid IDXGIDevice = new("54ec77fa-1377-44e6-8c32-88fd5f44c84c");
    internal static Guid IInspectable = new("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90");
    internal static Guid ID3D11Resource = new("dc8e63f3-d12b-4952-b47b-5e45026a862d");
    internal static Guid IDXGIAdapter3 = new("645967A4-1392-4310-A798-8053CE3E93FD");
    internal static Guid ID3D11Device = new("db6f6ddb-ac77-4e88-8253-819df9bbf140");
    internal static Guid ID3D11Texture2D = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

    [DllImport(
        "d3d11.dll",
        EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice",
        SetLastError = true,
        CharSet = CharSet.Unicode,
        ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall
        )]
    internal static extern uint CreateDirect3D11DeviceFromDXGIDevice(nint dxgiDevice, out nint graphicsDevice);

    [DllImport(
        "d3d11.dll",
        EntryPoint = "CreateDirect3D11SurfaceFromDXGISurface",
        SetLastError = true,
        CharSet = CharSet.Unicode,
        ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall
        )]
    internal static extern uint CreateDirect3D11SurfaceFromDXGISurface(nint dxgiSurface, out nint graphicsSurface);

    private static readonly uint DxgiErrorUnsupported = 0x887A0004;

    private static ID3D11Device CreateD3DDevice(D3D_DRIVER_TYPE driverType, D3D11_CREATE_DEVICE_FLAG flags)
    {
        unsafe
        {
            D3D11CreateDevice(null, driverType, new EmptyHandle(), flags, null, D3D11_SDK_VERSION, out var device, null, out var _);
            return device;
        }
    }

    public static ID3D11Device CreateD3DDevice()
    {
        ID3D11Device d3dDevice = null;
        var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;
        try
        {
            d3dDevice = CreateD3DDevice(D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, flags);
        }
        catch (Exception ex)
        {
            if (ex.HResult != (int)DxgiErrorUnsupported)
            {
                throw;
            }
        }

        d3dDevice ??= CreateD3DDevice(D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_WARP, flags);

        return d3dDevice;
    }

    public static IDirect3DDevice CreateDevice()
    {
        var d3dDevice = CreateD3DDevice();
        return CreateDirect3DDeviceFromD3D11Device(d3dDevice);
    }

    public static IDirect3DDevice CreateDirect3DDeviceFromD3D11Device(ID3D11Device d3dDevice)
    {
        var dxgiDevice = d3dDevice.As<IDXGIDevice>();
        Windows.Win32.PInvoke.CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice, out var raw);
        var rawPtr = Marshal.GetIUnknownForObject(raw);
        var result = MarshalInterface<IDirect3DDevice>.FromAbi(rawPtr);
        Marshal.Release(rawPtr);
        return result;
    }

    public static IDirect3DSurface CreateDirect3DSurfaceFromD3D11Texture2D(ID3D11Texture2D texture)
    {
        var dxgiSurface = texture.As<IDXGISurface>();
        Windows.Win32.PInvoke.CreateDirect3D11SurfaceFromDXGISurface(dxgiSurface, out var raw);
        var rawPtr = Marshal.GetIUnknownForObject(raw);
        var result = MarshalInterface<IDirect3DSurface>.FromAbi(rawPtr);
        Marshal.Release(rawPtr);
        return result;
    }

    public static T GetDXGIInterfaceFromObject<T>(object obj)
    {
        var access = obj.As<Windows.Win32.System.WinRT.Direct3D11.IDirect3DDxgiInterfaceAccess>();
        object result = null;
        unsafe
        {
            var guid = typeof(T).GUID;
            var guidPointer = (Guid*)Unsafe.AsPointer(ref guid);
            access.GetInterface(guidPointer, out result);
        }
        return result.As<T>();
    }

    public static ID3D11Device GetD3D11Device(IDirect3DDevice device)
    {
        return GetDXGIInterfaceFromObject<ID3D11Device>(device);
    }

    public static ID3D11Texture2D GetD3D11Texture2D(IDirect3DSurface surface)
    {
        return GetDXGIInterfaceFromObject<ID3D11Texture2D>(surface);
    }

    public static IDirect3DSurface CreateDirect3DSurfaceFromSharpDXTexture(Texture2D texture)
    {
        IDirect3DSurface surface = null;

        using (var dxgiSurface = texture.QueryInterface<SharpDX.DXGI.Surface>())
        {
            uint hr = CreateDirect3D11SurfaceFromDXGISurface(dxgiSurface.NativePointer, out nint pUnknown);

            if (hr == 0)
            {
                surface = MarshalInterface<IDirect3DSurface>.FromAbi(pUnknown);
                Marshal.Release(pUnknown);
            }
        }

        return surface;
    }

    public static SharpDX.Direct3D11.Device CreateSharpDXDevice(IDirect3DDevice device)
    {
        var access = device.As<IDirect3DDxgiInterfaceAccess>();
        var d3dPointer = access.GetInterface(ID3D11Device);
        var d3dDevice = new SharpDX.Direct3D11.Device(d3dPointer);

        return d3dDevice;
    }

    public static Texture2D CreateSharpDXTexture2D(IDirect3DSurface surface)
    {
        var access = surface.As<IDirect3DDxgiInterfaceAccess>();
        var d3dPointer = access.GetInterface(ID3D11Texture2D);
        var d3dSurface = new Texture2D(d3dPointer);
        return d3dSurface;
    }


    public enum D2D1_FACTORY_TYPE
    {
        SingleThreaded = 0,
        MultiThreaded = 1
    }

    public enum D2D1_DEBUG_LEVEL
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Information = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D1_FACTORY_OPTIONS
    {
        public D2D1_FACTORY_TYPE Type;
        public D2D1_DEBUG_LEVEL DebugLevel;
    }
}
