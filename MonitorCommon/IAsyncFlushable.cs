using System.Threading.Tasks;

namespace MonitorCommon
{
    public interface IAsyncFlushable
    {
        Task Flush();
    }
}