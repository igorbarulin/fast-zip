using System;
using System.Collections.Generic;
using System.Threading;
using MultiThreadGzip.Helpers;
using MultiThreadGzip.Interfaces;

namespace MultiThreadGzip.Processors
{
    public class ProcessorQueue : IProcessorQueue
    {
        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public bool ReadyToEnqueue
        {
            get { return _isRunning && _taskCount < _buffer; }
        }
        
        private bool _isRunning = true;
        
        private volatile int _taskCount;
        private readonly int _buffer;
        
        private readonly EventWaitHandle _whEnqueue = new AutoResetEvent(false);
        private readonly EventWaitHandle _whWork = new AutoResetEvent(false);
        
        private readonly object _locker = new object();
        private readonly Queue<object> _tasks = new Queue<object>();
        
        private readonly ITaskHandler _taskHandler;
        private readonly IProcessorQueue _nextProcessorQueue;

        private readonly Action<Exception> _onException;
        
        private readonly Thread _worker;

        
        public ProcessorQueue(ITaskHandler taskHandler, IProcessorQueue nextProcessorQueue, Action<Exception> onException, int buffer)
        {
            if (taskHandler == null)
            {
                throw new ArgumentNullException(MemberInfoGetting.GetMemberName(() => taskHandler));
            }
            
            if (buffer < 1)
            {
                throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => buffer),
                    "Cannot be less than one");
            }

            _taskHandler = taskHandler;
            _nextProcessorQueue = nextProcessorQueue;
            _onException = onException;
            _buffer = buffer;
            _worker = new Thread(Work);
            _worker.Start();
        }

        public bool TryEnqueue(object task)
        {
            if (ReadyToEnqueue && task != null)
            {
                Interlocked.Increment(ref _taskCount);

                lock (_locker)
                {
                    _tasks.Enqueue(task);
                }
                
                _whWork.Set();

                return true;
            }
            
            return false;
        }

        public void WaitForReady()
        {
            if (!_isRunning)
            {
                throw new OperationCanceledException("Queue was canceled");
            }

            if (ReadyToEnqueue)
            {
                return;
            }
            
            _whEnqueue.WaitOne();
            
            if (!_isRunning)
            {
                throw new OperationCanceledException("Queue was canceled");
            }
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
            _isRunning = false;
        }

        private void Work()
        {
            while (_isRunning)
            {
                object task;

                if (TryDequeue(out task))
                {
                    if (task == null)
                    {
                        return;
                    }
                    
                    Execute(task);
                    
                    Interlocked.Decrement(ref _taskCount);
                    
                    _whEnqueue.Set();
                }
                else
                {
                    _whWork.WaitOne();
                }
            }
        }
        
        private bool TryDequeue(out object task)
        {
            lock (_locker)
            {
                if (_tasks.Count > 0)
                {
                    task = _tasks.Dequeue();
                    return true;
                }
            }

            task = null;
            return false;
        }

        private void Execute(object task)
        {
            try
            {
                var result = _taskHandler.Execute(task);
                    
                if (_nextProcessorQueue != null && result.Data != null)
                {
                    _nextProcessorQueue.Enqueue(result.Data);
                }
            }
            catch (Exception e)
            {
                if (_onException != null)
                {
                    _onException(e);
                }
            }
        }

        public void Dispose()
        {
            lock (_locker)
            {
                _tasks.Enqueue(null);
            }
            
            _whWork.Set();
            _worker.Join();

            _whEnqueue.Close();
            _whWork.Close();
        }
    }
}