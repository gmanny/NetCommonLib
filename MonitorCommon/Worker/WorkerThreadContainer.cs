using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace MonitorCommon.Worker
{
    internal static class WorkerThreadContainer
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")]
        public static extern IntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);
    }

    public class WorkerThreadContainer<TService, TTask>
    {
        private readonly ILogger logger;
        private readonly int threadCount;
        private readonly int[] processorIdx;
        private readonly Action<TTask> runTask;
        private readonly bool logStartEnd;

        private readonly ConcurrentQueue<TTask> taskQueue = new ConcurrentQueue<TTask>();

        public WorkerThreadContainer(ILogger logger, int threadCount, int[] ignoredCores, Action<TTask> runTask, bool logStartEnd = false)
        {
            this.logger = logger;
            this.threadCount = threadCount;
            this.runTask = runTask;
            this.logStartEnd = logStartEnd;

            if (threadCount > Environment.ProcessorCount - ignoredCores.Length)
            {
                logger.LogCritical($"The number of threads specified is bigger than the number of CPUs ({Environment.ProcessorCount}) minus the number of ignored cores ({ignoredCores.Length}, [{String.Join(",", ignoredCores)}])");
                return;
            }

            processorIdx = Enumerable.Range(1, Environment.ProcessorCount)
                .Select(threadId => threadId <= Environment.ProcessorCount / 2 ? (threadId - 1) * 2 : (threadId - 1) * 2 - Environment.ProcessorCount + 1)
                .Where(x => !ignoredCores.Contains(x))
                .ToArray();
        }
        
        public ConcurrentQueue<TTask> TaskQueue => taskQueue;

        public Task Start()
        {
            // init worker threads
            Task[] workerThreadEndings = Enumerable.Range(1, threadCount).Select(x =>
            {
                TaskCompletionSource<Unit> finished = new TaskCompletionSource<Unit>();
                var thread = new Thread(() =>
                {
                    try
                    {
                        WorkerProc(x);
                    }
                    finally
                    {
                        finished.SetResult(Unit.Default);
                    }
                })
                {
                    Name = $"{typeof(TService).Name}-Worker-{x}",
                    IsBackground = true
                };

                thread.Start();

                return (Task) finished.Task;
            }).ToArray();

            return Task.WhenAll(workerThreadEndings);
        }

        private void WorkerProc(int threadId)
        {
            int procNum = processorIdx[threadId - 1];
            try
            {
                WorkerThreadContainer.SetThreadAffinityMask(WorkerThreadContainer.GetCurrentThread(), new UIntPtr(1u << procNum));
            }
            catch
            {
                // ignored
            }

            if (logStartEnd)
            {
                logger.LogInformation($"{Thread.CurrentThread.Name} started...");
            }

            do
            {
                bool gotTask = taskQueue.TryDequeue(out TTask task);
                if (!gotTask)
                {
                    continue;
                }

                try
                {
                    runTask(task);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Task {task} finished with exception");
                }
            } while (!taskQueue.IsEmpty);

            if (logStartEnd)
            {
                logger.LogInformation($"{Thread.CurrentThread.Name} exiting due to empty queue.");
            }
        }
    }
}