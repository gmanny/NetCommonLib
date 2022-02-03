using System.Threading;

namespace MonitorCommon;

public class AtomicReference<T> where T : class
{
    private T value;

    public AtomicReference(T value)
    {
        this.value = value;
    }

    public T Value => value;

    public T Replace(T newValue) => Interlocked.Exchange(ref value, newValue);
}