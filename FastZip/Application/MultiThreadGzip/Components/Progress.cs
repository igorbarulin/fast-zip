using System;
using MultiThreadGzip.Interfaces;

namespace MultiThreadGzip.Components
{
    public class Progress : IProgress
    {
        public event Action<int> OnProgressChanged;
        public int CurrentProgress { get; private set; }

        public void SetProgress(int value)
        {
            CurrentProgress = value;

            if (OnProgressChanged != null)
            {
                OnProgressChanged(value);
            }
        }

        public void Reset()
        {
            CurrentProgress = 0;
        }
    }
}