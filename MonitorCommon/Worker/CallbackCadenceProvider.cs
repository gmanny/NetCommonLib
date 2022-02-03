using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace MonitorCommon.Worker;

public class CallbackCadenceProvider<TState> : IDisposable
    where TState : ICadenceExecutionState
{
    private readonly TimeSpan period;
    private readonly ILogger logger;
    private readonly string name;
    private readonly TimeSpan taskDelay;
    private readonly int threadCount;
    private readonly bool logTime;
    private readonly Func<TState> makeState;
    private readonly AsyncSequentializer sequentializer;

    private readonly Timer timer;

    private readonly ConcurrentDictionary<Action<TState>, int> actions = new();

    private int iterationCount;
    private Task currentIteration = Task.CompletedTask;
    private bool canceled;

    public CallbackCadenceProvider(TimeSpan initialDelay, TimeSpan period, ILogger logger, string name, TimeSpan taskDelay, int threadCount = 1, bool logTime = false, Func<TState> makeState = null, AsyncSequentializer sequentializer = null)
    {
        this.period = period;
        this.logger = logger;
        this.name = name;
        this.taskDelay = taskDelay;
        this.threadCount = threadCount;
        this.logTime = logTime;
        this.makeState = makeState;
        this.sequentializer = sequentializer;

        timer = new Timer(OnTimer, null, initialDelay, TimeSpan.FromMilliseconds(-1));
    }

    public Task CurrentIteration => currentIteration;

    public event Action<TState> Timer
    {
        add { actions.AddOrUpdate(value, 1, (_, i) => i + 1); }
        remove
        {
            if (actions.TryRemove(value, out int occurrences))
            {
                if (occurrences != 1)
                {
                    logger.LogDebug($"Removed action {value} that was registered more than once");
                }
            }
            else
            {
                logger.LogDebug($"Removed action {value} that was not registered");
            }
        }
    }

    private void OnTimer(object _) => Run();

    private void Run()
    {
        if (sequentializer == null)
        {
            currentIteration = RunInternal();
        }
        else
        {
            currentIteration = sequentializer.NextAction(() => RunInternal().ToUnit());
        }
    }

    private async Task RunInternal()
    {
        try
        {
            List<Action<TState>> a = actions.Keys.ToList();
            if (a.NonEmpty())
            {
                DateTime start = DateTime.UtcNow;

                iterationCount++;

                TState state = makeState != null ? makeState() : default;
                if (state != null)
                {
                    state.Started();
                }

                Task end = ThreadUtil.RunMultiThread(a, x => x.Invoke(state), threadCount,
                    k => $"Cadence {name} * {iterationCount} runner #{k}", logger,
                    act => $"executing timer action {act}", "executing timer actions",
                    taskDelay);

                await end;

                if (state != null)
                {
                    state.Done();
                }

                if (logTime)
                {
                    logger.LogDebug($"Cadence {name} * {iterationCount} completed in {DateTime.UtcNow - start} ({a.Count} actions)");
                }
                else
                {
                    logger.LogTrace($"Cadence {name} * {iterationCount} completed in {DateTime.UtcNow - start} ({a.Count} actions)");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, $"Error executing timer iteration");
        }

        if (canceled)
        {
            return;
        }

        timer.Change(period, TimeSpan.FromMilliseconds(-1));
    }

    public void Dispose()
    {
        canceled = true;

        timer.Dispose();

        GC.SuppressFinalize(this);
    }
}