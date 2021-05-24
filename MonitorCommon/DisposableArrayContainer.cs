using System;

namespace MonitorCommon
{
    public class DisposableArrayContainer<T> : IDisposable where T : IDisposable
    {
        private readonly T[] items;

        public DisposableArrayContainer(T[] items)
        {
            this.items = items;
        }

        public void Dispose()
        {
            items.ForEach(i => i.Dispose());
        }
    }
}