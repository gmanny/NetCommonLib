using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MonitorCommon.Tasks;

// from https://putridparrot.com/blog/replacing-multiple-configureawait-with-the-synchronizationcontextremover/
public struct SynchronizationContextRemover : INotifyCompletion
{
    public bool IsCompleted => SynchronizationContext.Current == null;

    public void OnCompleted(Action continuation)
    {
        SynchronizationContext prevContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            continuation();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevContext);
        }
    }

    public SynchronizationContextRemover GetAwaiter()
    {
        return this;
    }

    public void GetResult() { }
}