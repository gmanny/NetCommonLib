using System;
using System.Threading;

namespace MonitorCommon
{
    public class DelegateDisposable : IDisposable
    {
        private readonly Action action;

        private int counter;

        public DelegateDisposable(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref counter) == 1)
            {
                action();
            }
        }
    }
}