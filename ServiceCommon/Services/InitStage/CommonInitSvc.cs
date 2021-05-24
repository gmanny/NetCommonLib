using Microsoft.Extensions.Logging;

namespace Monitor.ServiceCommon.Services.InitStage
{
    public abstract class CommonInitSvc<TResource> : CommonInitRunningSvc<TResource>
    {
        protected CommonInitSvc(ILogger logger, string serviceId = null) : base(logger, serviceId)
        {
            FinishedSuccess();
        }
    }
}