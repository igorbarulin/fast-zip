using System;

namespace MultiThreadGzip.Components
{
    public class CompressionResult
    {
        public bool HaveError
        {
            get { return ErrorCode != 0; }
        }
        
        public Exception Exception { get; private set; }
        public ErrorCode ErrorCode { get; private set; }
        
        public ZipMode ZipMode { get; private set; }
        
        public string InputFileName { get; private set; }
        public long InputFileLength { get; private set; }
        
        public string OutputFileName { get; private set; }
        public long OutputFileLength { get; private set; }
        
        public TimeSpan LeadTime { get; private set; }

        public CompressionResult(Exception exception, ErrorCode errorCode, ZipMode zipMode, 
            string inputFileName, long inputFileLength, string outputFileName, long outputFileLength, TimeSpan leadTime)
        {
            Exception = exception;
            ErrorCode = errorCode;
            
            ZipMode = zipMode;
            
            InputFileName = inputFileName;
            InputFileLength = inputFileLength;
            
            OutputFileName = outputFileName;
            OutputFileLength = outputFileLength;
            
            LeadTime = leadTime;
        }
    }
}