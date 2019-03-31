using Loggers.Components;

namespace Loggers
{
    public class NullableLogger : ILogger
    {
        public void Log(LogEntry logEntry)
        {
        }
    }
}