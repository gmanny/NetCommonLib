using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Monitor.ServiceCommon.Services;

public class TimePrecisionSvc
{
    private readonly ILogger logger;

    public TimePrecisionSvc(ILogger logger)
    {
        this.logger = logger;

        Thread testThread = new(TestTimePrecision)
        {
            Name = "Time precision test",
            IsBackground = true
        };

        testThread.Start();
    }

    private void TestTimePrecision()
    {
        TimeSpan testTime = TimeSpan.FromSeconds(5);

        DateTime start = DateTime.UtcNow;
        int changeCount = 0;
        double jumpSum = 0;

        DateTime dt = DateTime.UtcNow;
        while (changeCount < 5 || dt - start < testTime)
        {
            DateTime nw = DateTime.UtcNow;
            if (nw.Ticks != dt.Ticks)
            {
                changeCount++;
                jumpSum += (nw - dt).TotalMilliseconds;
            }

            dt = nw;
        }

        logger.LogInformation($"Time precision is {jumpSum / changeCount:0.0#####} ms (over {changeCount} changes / {(dt-start).TotalSeconds:0.0##} seconds tested)");
    }
}