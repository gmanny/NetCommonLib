using System.Threading.Tasks;

namespace MonitorCommon
{
    public interface IAsyncFlushable
    {
        Task Flush();
    }

    public interface IAsyncFlushable<TState> : IAsyncFlushable
    {
        Task Flush(TState state);
    }
}