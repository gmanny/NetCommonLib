using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Monitor.ServiceCommon.Services.DiEager;
using Monitor.ServiceCommon.Util;
using MonitorCommon;
using Ninject.Modules;

namespace Monitor.ServiceCommon.Services.InitStage.PostActivated;

public class PaInitStage<TResource>
{
    private readonly TaskCompletionSource<Unit> stageDone = new();

    public PaInitStage(PaInitSignal<TResource> signal, ILogger logger)
    {
        if (!signal.Register(stageDone.Task))
        {
            logger.LogWarning($"Init stage for {typeof(TResource).Name} was registered too late");
        }
    }

    public void StageDone()
    {
        stageDone.TrySetResult(Unit.Default);
    }
}

public interface IPaInitSignal
{
    void ActivationDone();
}

public class PaInitSignal<TResource> : IPaInitSignal
{
    private readonly ILogger logger;

    private readonly List<Task> allStages = new();
    private volatile bool isDone;

    private readonly TaskCompletionSource<Unit> allDone = new();

    public PaInitSignal(ILogger logger)
    {
        this.logger = logger;
    }

    public Task AllInited => allDone.Task;

    public bool Register(Task stageDoneTask)
    {
        allStages.Add(stageDoneTask);

        return !isDone;
    }

    public static void Bind(NinjectModule module)
    {
        module.Bind<PaInitStage<TResource>>().ToSelf();
        module.Bind<PaInitSignal<TResource>>().ToSelf().AsEagerSingleton();
        module.Bind<IPaInitSignal>().ToExisting().Singleton<PaInitSignal<TResource>>();
    }

    public async void ActivationDone()
    {
        isDone = true;

        logger.LogTrace($"Init signal for {typeof(TResource).Name} created with {allStages.Count} stages");

        if (allStages.NonEmpty())
        {
            await Task.WhenAll(allStages);
        }

        logger.LogTrace($"Init signal for {typeof(TResource).Name} finished with {allStages.Count} stages");

        allDone.TrySetResult(Unit.Default);
    }
}

public class PaInitSvc
{
    private readonly List<IPaInitSignal> allSignals;

    public PaInitSvc(IEnumerable<IPaInitSignal> allSignals)
    {
        this.allSignals = allSignals.ToList();
    }

    public void AllDone()
    {
        foreach (IPaInitSignal signal in allSignals)
        {
            signal.ActivationDone();
        }
    }
}