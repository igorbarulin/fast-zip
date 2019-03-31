using System;

namespace Loggers.Components
{
    public static class LogExtensions
    {
        public static void LogError(this ILogger logger, Exception err, string additionalMessage)
        {
            logger.Log(new LogEntry(LogCategory.Error, err.ToString(), additionalMessage, null));
        }
        
        public static void LogError(this ILogger logger, Exception err)
        {
            logger.LogError(err.ToString());
        }
        
        public static void LogError(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogCategory.Error, message, args);
        }
        
        public static void LogWarring(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogCategory.Warning, message, args);
        }

        public static void LogInfo(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogCategory.Info, message, args);
        }

        public static void Log(this ILogger logger, string category, string message, params object[] args)
        {
            logger.Log(new LogEntry(category, message, null, args));
        }
    }
}