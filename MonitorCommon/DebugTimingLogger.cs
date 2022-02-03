using System;
using Microsoft.Extensions.Logging;

namespace MonitorCommon;

public class DebugTimingLogger : IDisposable
{
    private readonly ILogger logger;
    private readonly string operation;
    private readonly int start;

    public DebugTimingLogger(ILogger logger, string operation)
    {
        this.logger = logger;
        this.operation = operation;

        start = Environment.TickCount;
    }


    public void Dispose()
    {
        logger.LogInformation($"{operation} took {(Environment.TickCount - start) / 1000.0:#.00}s");
    }
}