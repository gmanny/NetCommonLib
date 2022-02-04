using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MonitorCommon;

namespace Monitor.ServiceCommon.Services;

public class ProcessLifetimeSvc
{
    private readonly ConcurrentDictionary<Func<Task>, Unit> handlers = new();

    private ILogger? logger;
    private int done;

    public ProcessLifetimeSvc()
    {
        Console.CancelKeyPress += OnConsoleCancel;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    public bool IsExiting => done != 0;

    // logger is set in this backwards manner because the LoggingSvc has this service as a dependency, and getting a logger thru constructor leads to circular dependencies
    internal void SetLogger(ILogger l) => logger = l;

    public event Func<Task> ApplicationStop
    {
        add => handlers.TryAdd(value, Unit.Default);
        remove => handlers.TryRemove(value, out _);
    }

    private void OnConsoleCancel(object? sender, ConsoleCancelEventArgs e)
    {
        RunHandlers("console cancel");
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        RunHandlers("process exit");
    }

    private void RunHandlers(string place)
    {
        if (handlers.NonEmpty() && Interlocked.Exchange(ref done, 1) == 0)
        {
            logger?.LogInformation($"Running {handlers.Count} app exit handlers at {place}");

            Task.WhenAll(handlers.Keys.Select(h => h())).Wait();

            logger?.LogInformation($"All {handlers.Count} app exit handlers finished successfully");
        }
    }
}