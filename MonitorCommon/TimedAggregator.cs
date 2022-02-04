using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorCommon;

// this class is not thread-safe
public class TimedAggregator<T, TState> : IAsyncFlushable
{
    private readonly Action<(long time, List<T> items, TState state)> onPush;
    private readonly TState state;

    private long time;
    private List<T> items = new();

    public TimedAggregator(Action<(long time, List<T> items, TState state)> onPush, TState state)
    {
        this.onPush = onPush;
        this.state = state;
    }

    public void Next(long newTime, List<T> addItems)
    {
        if (addItems.IsEmpty())
        {
            onPush((time, items, state));

            time += 1;
            items = addItems;
            return;
        }

        if (newTime == time)
        {
            items.AddRange(addItems);
        }
        else
        {
            onPush((time, items, state));

            time = newTime;
            items = addItems;
        }
    }

    public Task Flush()
    {
        Next(time+1, new List<T>());

        return Task.CompletedTask;
    }
}