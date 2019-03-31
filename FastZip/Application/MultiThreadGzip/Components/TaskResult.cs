namespace MultiThreadGzip.Components
{
    public class TaskResult
    {
        public object Data { get; private set; }

        public TaskResult()
        {
        }
        
        public TaskResult(object data)
        {
            Data = data;
        }
    }
}