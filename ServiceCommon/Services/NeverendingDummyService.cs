using System.Threading.Tasks;
using LanguageExt;

namespace Monitor.ServiceCommon.Services
{
    public class NeverendingDummyService : IRunningService
    {
        private readonly TaskCompletionSource<Unit> notFinished = new TaskCompletionSource<Unit>();

        public string ServiceId => GetType().Name;

        public Task Finished => notFinished.Task;
    }
}