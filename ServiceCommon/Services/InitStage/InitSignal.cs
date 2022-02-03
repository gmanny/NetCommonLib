using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Monitor.ServiceCommon.Services.DiEager;
using MonitorCommon.Tasks;
using Ninject.Modules;

namespace Monitor.ServiceCommon.Services.InitStage;

public class InitSignal<TResource>
{
    private readonly ILogger logger;

    private TaskCompletionSource<Unit> allInited = new();

    public InitSignal(ILogger logger)
    {
        this.logger = logger;
    }

    public Task AllInited => allInited.Task;

    public void SignalTotalCompletion(ITaskResult<Unit> result)
    {
        allInited.Complete(result);

        logger.LogTrace($"Signaling init completion for {typeof(TResource).Name}");
    }

    public static void Bind(NinjectModule module)
    {
        module.Bind<InitSignal<TResource>>().ToSelf().InSingletonScope();
        module.Bind<InitServiceHub<TResource>>().ToSelf().AsEagerSingleton();
    }
}