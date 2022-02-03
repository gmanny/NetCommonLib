using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MonitorCommon.Tasks;

namespace MonitorCommon;

public class AsyncSequentializer
{
    private record TaskContainer(Task WithoutDelay, Task WithDelay);

    private readonly TimeSpan delay; // between tasks

    private TaskContainer tail;

    public AsyncSequentializer(TimeSpan delay = default)
    {
        this.delay = delay;
    }

    public async Task<T> NextAction<T>(Func<Task<T>> work, bool noDelay = false)
    {
        TaskCompletionSource<T> next = new TaskCompletionSource<T>();
        Task<T> ret = next.Task;
        TaskContainer ntx = new(ret, delay.Ticks > 0 ? ret.FlatMapAll(_ => Task.Delay(delay).ToUnit()) : ret.ToUnit());

        TaskContainer p = Interlocked.Exchange(ref tail, ntx);
        if (p != null)
        {
            Task prev = noDelay ? p.WithoutDelay : p.WithDelay;
            try
            {
                await prev;
            }
            catch
            {
                // ignored
            }
        }

        try
        {
            T res = await work();

            next.TrySetResult(res);

            return res;
        }
        catch (Exception e)
        {
            next.TrySetException(e);

            throw;
        }
    }
}