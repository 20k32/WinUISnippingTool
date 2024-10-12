using System;

namespace WinUISnippingTool.Models.VideoCapture;

internal class MultithreadLock : IDisposable
{
    private SharpDX.Direct3D11.Multithread multithread;
    
    public MultithreadLock(SharpDX.Direct3D11.Multithread multithread)
    {
        this.multithread = multithread;
        this.multithread?.Enter();
    }

    public void Dispose()
    {
        multithread?.Leave();
        multithread = null;
    }

}
