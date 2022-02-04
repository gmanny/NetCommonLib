using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MonitorCommon.Threading;

public interface INotificationRecord
{
    void Notify();
}

public class NotificationRunner
{
    private readonly string name;
    private readonly ILogger logger;

    private readonly ConcurrentQueue<INotificationRecord> queue = new();

    private int started;

    public NotificationRunner(string name, ILogger logger)
    {
        this.name = name;
        this.logger = logger;
    }

    public void Add(INotificationRecord record) => queue.Enqueue(record);

    public bool TryStart()
    {
        if (Interlocked.Exchange(ref started, 1) != 0)
        {
            return false;
        }

        Thread t = new(ThreadRun)
        {
            IsBackground = true,
            Name = name,
            Priority = ThreadPriority.BelowNormal
        };

        t.Start();

        return true;
    }

    private void ThreadRun()
    {
        try
        {
            int waits = 0;

            while (true)
            {
                if (!queue.TryDequeue(out INotificationRecord? rec))
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(Math.Min(4, waits / 100)));

                    waits++;

                    continue;
                }

                waits = 0;

                try
                {
                    rec.Notify();
                }
                catch (Exception e)
                {
                    logger.LogDebug(e, $"Error running notification {rec}");
                }
            }
        }
        finally
        {
            logger.LogDebug($"Notification thread {name} finished");
        }
    }
}