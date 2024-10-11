using System;
using System.Runtime.InteropServices;


namespace WinUISnippingTool.Models.VideoCapture;

internal class EmptyHandle : SafeHandle
{
    public EmptyHandle() : base(IntPtr.Zero, false)
    {
    }

    public override bool IsInvalid => true;

    protected override bool ReleaseHandle()
    {
        return true;
    }
}
