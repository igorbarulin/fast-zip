using System;

namespace MultiThreadGzip.Interfaces
{
    public interface IProgress
    {
        event Action<int> OnProgressChanged;
        
        int CurrentProgress { get; }
    }
}