using MultiThreadGzip.Components;
using MultiThreadGzip.Processors;

namespace MultiThreadGzip.Interfaces
{
    public interface ITaskHandler
    {
        TaskResult Execute(object task);
    }
}