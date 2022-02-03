using System.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Monitor.ServiceCommon.Services;

public class GcSettingsSvc
{
    public GcSettingsSvc(IConfiguration config, ILogger logger)
    {
        IConfigurationSection svcConf = config.GetSection("gc");
        if (svcConf.Exists())
        {
            if (svcConf.GetValue<bool>("low-latency"))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
        }

        logger.LogInformation($"Using {GCSettings.LatencyMode} GC latency mode, GC type = {(GCSettings.IsServerGC ? "Server" : "Workstation")}");
    }
}