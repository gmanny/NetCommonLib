using System.Threading.Tasks;

namespace Monitor.ServiceCommon.Services.InitStage
{
    public interface IInitStageService<TResource> : IService
    {
        Task InitComplete { get; }
    }
}