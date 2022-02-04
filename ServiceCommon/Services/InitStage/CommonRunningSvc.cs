using Microsoft.Extensions.Logging;

namespace Monitor.ServiceCommon.Services.InitStage;

public interface IDummyInitResource { }

public abstract class CommonRunningSvc : CommonInitRunningSvc<IDummyInitResource>
{
    protected CommonRunningSvc(ILogger logger, string? serviceId = null) : base(logger, serviceId)
    {
        InitSuccess();
    }
}