using System.Threading.Tasks;

namespace Monitor.ServiceCommon.Services
{
    public interface IRunningService : IService
    {
        Task Finished { get; }
    }
}