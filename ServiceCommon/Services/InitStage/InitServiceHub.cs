using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MonitorCommon.Tasks;

namespace Monitor.ServiceCommon.Services.InitStage;

public class InitServiceHub<TResource>
{
    public InitServiceHub(IEnumerable<IInitStageService<TResource>> services, InitSignal<TResource> signal)
    {
        Task allDone = services.Aggregate(Task.CompletedTask, (task, s) => task.MapAsync(() => s.InitComplete.ToUnit()));

        allDone.OnComplete(signal.SignalTotalCompletion);
    }
}