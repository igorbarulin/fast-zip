using System.IO;
using Loggers.Components;

namespace Loggers
{
    public class FileLogger : ILogger
    {
        private readonly object _locker = new object();

        private readonly string _fileName;

        public FileLogger(string fileName)
        {
            _fileName = fileName;
        }
        
        public void Log(LogEntry logEntry)
        {
            lock (_locker)
            {
                using (var file = File.AppendText(_fileName))
                {
                    file.WriteLine(logEntry.FormattedMessage);
                }
            }
        }
    }
}