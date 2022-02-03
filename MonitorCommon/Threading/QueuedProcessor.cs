using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DequeNet;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MonitorCommon.Tasks;

namespace MonitorCommon.Threading;

public class QueuedProcessor<TWork, TResult>
{
    private readonly Func<TWork, CancellationToken, Task<TResult>> workProcessor;
    private readonly int threadCount;
    private readonly string name;
    private readonly ILogger logger;
    private readonly TimeSpan intraTaskDelay;
    private readonly TimeSpan workerThreadSleepDelay;

    private readonly ConcurrentDictionary<TWork, QueueRecord> workCache = new();
    private readonly ConcurrentDeque<QueueRecord> queue = new();
    private readonly object sync = new();

    public QueuedProcessor(Func<TWork, CancellationToken, Task<TResult>> workProcessor, int threadCount, string name, ILogger logger, TimeSpan intraTaskDelay, TimeSpan workerThreadSleepDelay)
    {
        this.workProcessor = workProcessor;
        this.threadCount = threadCount;
        this.name = name;
        this.logger = logger;
        this.intraTaskDelay = intraTaskDelay;
        this.workerThreadSleepDelay = workerThreadSleepDelay;

        SpawnSleepingThread();
    }

    public Task<TResult> AddWork(TWork work, bool first = false, CancellationToken ct = default)
    {
        TaskCompletionSource<TResult> result = new();

        // this action will run earlier
        if (!first && workCache.TryGetValue(work, out QueueRecord workItem))
        {
            return workItem.Result.Task;
        }

        lock (sync)
        {
            // cancel the duplicate work that'll run later
            if (first && workCache.TryGetValue(work, out workItem))
            {
                workItem.Cts.Cancel();

                // "promote" the task completion source to the front of the queue
                (result, workItem.Result) = (workItem.Result, result);
            }

            CancellationTokenSource cts = new();
            ct.Register(() => cts.Cancel());

            QueueRecord item = new(work, result, cts);

            workCache[work] = item;

            if (first)
            {
                queue.PushLeft(item);
            }
            else
            {
                queue.PushRight(item);
            }

            Monitor.PulseAll(sync);
        }

        return result.Task;
    }

    private async Task<bool> HandleWorkItem(QueueRecord rec)
    {
        try
        {
            if (rec.Result.Task.IsCompleted)
            {
                return false;
            }

            if (rec.Cts.IsCancellationRequested)
            {
                rec.Result.TrySetCanceled(rec.Cts.Token);
                return false;
            }

            try
            {
                TResult res = await workProcessor(rec.Work, rec.Cts.Token);

                rec.Result.TrySetResult(res);
            }
            catch (Exception e)
            {
                rec.Result.TrySetException(e);
            }

            return true;
        }
        finally
        {
            workCache.TryRemove(rec.Work, out _);
        }
    }

    private async void SpawnWorkerTasks()
    {
        await SpawnWorkerTasksInternal();

        logger.LogDebug($"{name} worker task pool finished");

        SpawnSleepingThread();
    }

    private async Task SpawnWorkerTasksInternal()
    {
        int parkedThreads = 0;
        TaskCompletionSource<Unit> allDone = new();
        CancellationTokenSource cts = new();
            
        // todo: this is open to the risk of all but 1 thread finishing when many more items are added to the queue
        async Task StartWorker()
        {
            if (queue.TryPopLeft(out QueueRecord task))
            {
                bool taskRan;
                try
                {
                    taskRan = await HandleWorkItem(task);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, $"Error processing task {task.Work}");
                    taskRan = true;
                }

                if (taskRan)
                {
                    try
                    {
                        await Task.Delay(intraTaskDelay, task.Cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }
            else
            {
                int totalParkedThreads = Interlocked.Increment(ref parkedThreads);
                if (totalParkedThreads == threadCount)
                {
                    allDone.TrySetResult(Unit.Default);
                    cts.Cancel();
                    return;
                }

                await Task.WhenAny(
                    allDone.Task.Map(_ => true),
                    Task.Delay(workerThreadSleepDelay, cts.Token).MapAll(_ => false)
                );

                if (allDone.Task.IsCompleted)
                {
                    return;
                }

                Interlocked.Decrement(ref parkedThreads);
            }

            await Task.Yield();
            await StartWorker().ConfigureAwait(false);
        }

        async Task DoAllWork()
        {
            Task[] workers = new Task[threadCount];

            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = StartWorker();
            }

            foreach (Task worker in workers)
            {
                await worker;
            }

            allDone.TrySetResult(Unit.Default);
        }

        // work starts here
        await DoAllWork();
    }

    private void SpawnSleepingThread()
    {
        Thread t = new(SleepingThreadRun)
        {
            Name = $"{name} waiter",
            IsBackground = true
        };

        t.Start();
    }

    private void SleepingThreadRun()
    {
        while (true)
        {
            lock (sync)
            {
                if (!queue.IsEmpty)
                {
                    SpawnWorkerTasks();
                    return;
                }

                Monitor.Wait(sync);
            }
        }
    }

    private record QueueRecord(TWork Work, TaskCompletionSource<TResult> Result, CancellationTokenSource Cts)
    {
        public TaskCompletionSource<TResult> Result { get; set; } = Result;
    }
}