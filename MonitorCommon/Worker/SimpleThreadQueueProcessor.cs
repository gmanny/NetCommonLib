using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MonitorCommon.Worker;

public class SimpleThreadQueueProcessor<TTask>
{
    private readonly Action<TTask> runTask;
    private readonly string threadName;
    private readonly ILogger logger;
    private readonly Action? idleAction;

    private readonly ConcurrentQueue<TTask> taskQueue = new();

    public SimpleThreadQueueProcessor(Action<TTask> runTask, string threadName, ILogger logger, Action? idleAction = null)
    {
        this.runTask = runTask;
        this.threadName = threadName;
        this.logger = logger;
        this.idleAction = idleAction;

        Thread thread = new(() =>
        {
            try
            {
                WorkerProc();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Queue processor {threadName} loop failed");
            }
        })
        {
            Name = threadName,
            IsBackground = true
        };

        thread.Start();
    }
        
    public ConcurrentQueue<TTask> TaskQueue => taskQueue;

    public void Post(TTask task) => taskQueue.Enqueue(task);

    private void WorkerProc()
    {
        SpinWait sw = new();

        do
        {
            TTask? task;
            while (!taskQueue.TryDequeue(out task))
            {
                sw.SpinOnce();

                idleAction?.Invoke();
            }

            try
            {
                runTask(task);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Task {task} from {threadName} finished with exception");
            }
        } while (true);
    }
}