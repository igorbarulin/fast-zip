using System;
using Loggers.Components;

namespace Loggers
{
    public class ConsoleLogger : ILogger
    {
        private static readonly object StaticLocker = new object();

        public void Log(LogEntry logEntry)
        {
            WriteLine(logEntry.FormattedMessage, GetColor(logEntry.Category));
        }

        private static void WriteLine(string message, ConsoleColor color)
        {
            lock (StaticLocker)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        private static ConsoleColor GetColor(string category)
        {
            switch (category)
            {
                case LogCategory.Error:
                    return ConsoleColor.Red;
                case LogCategory.Warning:
                    return ConsoleColor.Yellow;
                default:
                    return ConsoleColor.White;
            }
        }
    }
}