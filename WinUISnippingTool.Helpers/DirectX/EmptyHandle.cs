using System;
using System.Runtime.InteropServices;

namespace WinUISnippingTool.Helpers.DirectX;

internal class EmptyHandle : SafeHandle
{
    public EmptyHandle() : base(nint.Zero, false)
    {
    }

    public override bool IsInvalid => true;

    protected override bool ReleaseHandle()
    {
        return true;
    }
}
