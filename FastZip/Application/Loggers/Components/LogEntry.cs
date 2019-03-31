using System;

namespace Loggers.Components
{
    public class LogEntry
    {
        public DateTime DateTime { get; private set; }
        public string Category { get; private set; }
        public string Message { get; private set; }
        public string AdditionalMessage { get; private set; }
        public object[] Args { get; private set; }

        public string FormattedMessage
        {
            get
            {
                return !string.IsNullOrEmpty(AdditionalMessage)
                    ? string.Format("{0}\n{1}", AdditionalMessage, FormatMessage())
                    : FormatMessage();
            }
        }

        public LogEntry(string category, string message, string additionalMessage, object[] args)
        {
            DateTime = DateTime.UtcNow;
            Category = category;
            Message = message;
            AdditionalMessage = additionalMessage;
            Args = args;
        }

        private string FormatMessage()
        {
            return Args == null || Args.Length == 0 ? Message : string.Format(Message, Args);
        }
    }
}