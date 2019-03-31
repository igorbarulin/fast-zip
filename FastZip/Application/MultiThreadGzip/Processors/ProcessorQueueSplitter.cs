using System;
using System.Linq;
using System.Threading;
using MultiThreadGzip.Helpers;
using MultiThreadGzip.Interfaces;

namespace MultiThreadGzip.Processors
{
    public class ProcessorQueueSplitter : IProcessorQueue
    {
        public bool IsRunning
        {
            get { return _queues.Any(q => q.IsRunning); }
        }
        
        public bool ReadyToEnqueue
        {
            get { return _queues.Any(q => q.ReadyToEnqueue); }
        }

        private readonly IProcessorQueue[] _queues;
        
        public ProcessorQueueSplitter(IProcessorQueue[] queues)
        {
            if (queues == null)
            {
                throw new ArgumentNullException(MemberInfoGetting.GetMemberName(() => queues));
            }

            if (!queues.Any())
            {
                throw new ArgumentException("Cannot be empty", MemberInfoGetting.GetMemberName(() => queues));
            }

            _queues = queues;
        }
        
        public void Enqueue(object task)
        {
            if (task == null)
            {
                throw new NullReferenceException(MemberInfoGetting.GetMemberName(() => task));
            }
            
            while (!TryEnqueue(task))
            {
                WaitForReady();
            }
        }

        public void Cancel()
        {
            foreach (var queue in _queues)
            {
                queue.Cancel();
            }
        }

        public bool TryEnqueue(object task)
        {
            return _queues.Any(q => q.TryEnqueue(task));
        }

        public void WaitForReady()
        {
            while (!ReadyToEnqueue)
            {
                if (!IsRunning)
                {
                    throw new OperationCanceledException("Queue was canceled");
                }
                
                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            foreach (var queue in _queues)
            {
                queue.Dispose();
            }
        }
    }
}