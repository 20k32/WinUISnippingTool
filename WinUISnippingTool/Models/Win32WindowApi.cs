using System;
using System.Runtime.InteropServices;


namespace WinUISnippingTool.Models;

internal static class Win32WindowApi
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public const int SwHide = 0;
    public const int SwShow = 5;
}
