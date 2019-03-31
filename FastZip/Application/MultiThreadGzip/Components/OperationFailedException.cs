using System;
using System.Text;

namespace MultiThreadGzip.Components
{
    public class OperationFailedException : Exception
    {
        public ErrorCode ErrorCode { get; private set; }

        public OperationFailedException(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("ERROR CODE: {0}", ErrorCode));
            sb.AppendLine(base.ToString());
            return sb.ToString();
        }
    }
}