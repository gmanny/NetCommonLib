using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitorCommon.Tasks;
using Newtonsoft.Json;

namespace Monitor.ServiceCommon.Services;

public abstract class CommonEnabledSvc : IRunningService
{
    protected readonly TaskScheduler scheduler;
    protected readonly ILogger logger;
    protected readonly JsonSerializer ser;

    protected readonly IConfigurationSection cfg;
    protected readonly bool isEnabled;

    protected abstract string ServiceConfigSection { get; }
    protected abstract string ServiceWorkDescription { get; }

    protected CommonEnabledSvc(IConfiguration config, TaskScheduler scheduler, ILogger logger, JsonSerializer ser)
    {
        this.scheduler = scheduler;
        this.logger = logger;
        this.ser = ser;
            
        cfg = config.GetSection(ServiceConfigSection);
        isEnabled = cfg.GetValue<bool>("enabled");
        if (!isEnabled)
        {
            Finished = Task.CompletedTask;
            return;
        }

        Task t = DoWork();

        Finished = t.Recover(e =>
        {
            logger.LogWarning(e, $"Failed to {ServiceWorkDescription}");
        });
    }

    public Task Finished { get; }

    public string ServiceId => GetType().Name;

    protected abstract Task DoWork();
}