using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace MonitorCommon;

public static class ThreadUtil
{
    public static Task RunMultiThread<TWork>(IList<TWork> workItems, Action<TWork> handleItem, int threadCount, Func<int, string> threadName, ILogger logger, Func<TWork, string> workActionDescr, string workOverallDescr, TimeSpan taskDelay)
    {
        void RunActions(int threadNum, int tc, TaskCompletionSource<Unit> finished)
        {
            try
            {
                for (int i = threadNum; i < workItems.Count; i += tc)
                {
                    TWork wi = workItems[i];

                    try
                    {
                        handleItem(wi);
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning(e, $"Error {workActionDescr(wi)}");
                    }

                    if (taskDelay.Ticks > 0)
                    {
                        Thread.Sleep(taskDelay);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Error {workOverallDescr} #{threadNum} / {tc}");
            }
            finally
            {
                finished.TrySetResult(Unit.Default);
            }
        }

        Thread[] threads = new Thread[threadCount];
        Task[] completed = new Task[threadCount];
        for (int i = 0; i < threads.Length; i++)
        {
            int k = i;
            var tcs = new TaskCompletionSource<Unit>();
            completed[i] = tcs.Task;

            threads[i] = new Thread(() => RunActions(k, threadCount, tcs));
            threads[i].Name = threadName(k);

            threads[i].Start();
        }

        return Task.WhenAll(completed);
    }
}