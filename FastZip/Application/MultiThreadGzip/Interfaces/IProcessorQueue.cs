using System;

namespace MultiThreadGzip.Interfaces
{
    public interface IProcessorQueue : IDisposable
    {
        bool IsRunning { get; }
        bool ReadyToEnqueue { get; }
        bool TryEnqueue(object task);
        void WaitForReady();
        void Enqueue(object task);
        void Cancel();
    }
}