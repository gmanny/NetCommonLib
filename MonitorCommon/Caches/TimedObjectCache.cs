using System;

namespace MonitorCommon.Caches;

public class TimedObjectCache<T> where T : notnull
{
    private readonly TimeSpan maxAge;
        
    private TimeBox<T> current = new(Epoch.Start, default!);

    public TimedObjectCache(TimeSpan maxAge)
    {
        this.maxAge = maxAge;
    }

    public T Get(Func<T> producer)
    {
        DateTime now = DateTime.Now;

        if (now - current.Time > maxAge)
        {
            lock (this)
            {
                if (now - current.Time > maxAge)
                {
                    current = new TimeBox<T>(now, producer());
                }
            }
        }

        return current.Value;
    }
}