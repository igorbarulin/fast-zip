using System;
using MultiThreadGzip.Interfaces;

namespace MultiThreadGzip.Components
{
    public class TaskHandler<T, TResult> : ITaskHandler where T : class
    {
        private readonly Func<T, TResult> _func;
        
        public TaskHandler(Func<T, TResult> func)
        {
            _func = func;
        }
        
        public TaskResult Execute(object task)
        {
            var t = task as T;

            var result = _func(t);
            return new TaskResult(result);
        }
    }

    public class TaskHandler<T> : ITaskHandler where T : class
    {
        private readonly Action<T> _action;

        public TaskHandler(Action<T> action)
        {
            _action = action;
        }

        public TaskResult Execute(object task)
        {
            var t = task as T;
            
            _action(t);
            return new TaskResult();
        }
    }
}