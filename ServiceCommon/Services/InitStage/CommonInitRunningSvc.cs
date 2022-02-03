using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MonitorCommon.Tasks;

namespace Monitor.ServiceCommon.Services.InitStage;

public abstract class CommonInitRunningSvc<TResource> : IInitStageService<TResource>, IRunningService
{
    private readonly ILogger logger;

    private readonly TaskCompletionSource<Unit> initComplete = new();
    private readonly TaskCompletionSource<Unit> finished = new();

    protected CommonInitRunningSvc(ILogger logger, string serviceId = null)
    {
        this.logger = logger;

        ServiceId = serviceId ?? GetType().Name;
    }

    public string ServiceId { get; }

    public Task InitComplete => initComplete.Task;
    public Task Finished => finished.Task;

    protected void InitSuccess() => initComplete.SetResult(Unit.Default);
    protected Task InitWith(Task t) => DoFinish(initComplete, t, "init");

    protected void FinishedSuccess() => finished.SetResult(Unit.Default);
    protected Task FinishWith(Task t) => DoFinish(finished, t, "work");

    private Task DoFinish(TaskCompletionSource<Unit> tcs, Task t, string taskName)
    {
        Task resultingTask = t.MapAll(r =>
        {
            tcs.Complete(r);

            return Unit.Default;
        });

        resultingTask.OnFailure(e =>
        {
            logger.LogWarning(e, $"{ServiceId} failed {taskName}");
        });

        resultingTask.OnCanceled(() =>
        {
            logger.LogWarning($"{ServiceId}'s {taskName} was canceled");
        });

        return resultingTask;
    }
}