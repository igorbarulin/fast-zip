using System;

namespace MultiThreadGzip.Components
{
    public class CompressionResultBuilder
    {
        public CompressionResult Result
        {
            get
            {
                return new CompressionResult(_exception, _errorCode, _zipMode, _inputFileName, _inputFileLength,
                    _outputFileName, _outputFileLength, _leadTime);
            }
        }
        
        private Exception _exception;
        private ErrorCode _errorCode;
        
        private readonly ZipMode _zipMode;
        
        private string _inputFileName;
        private long _inputFileLength;
        
        private string _outputFileName;
        private long _outputFileLength;
        
        private TimeSpan _leadTime;


        public CompressionResultBuilder(ZipMode zipMode)
        {
            _errorCode = ErrorCode.Success;
            _zipMode = zipMode;
        }

        public void SetException(Exception exception)
        {
            _exception = exception;
            ResolveErrorCode(exception, out _errorCode);
        }

        public void SetInputFileInfo(string inputFileName, long inputFileLength)
        {
            _inputFileName = inputFileName;
            _inputFileLength = inputFileLength;
        }

        public void SetOutputFileInfo(string outputFileName, long outputFileLength)
        {
            _outputFileName = outputFileName;
            _outputFileLength = outputFileLength;
        }

        public void SetLeadTime(TimeSpan leadTime)
        {
            _leadTime = leadTime;
        }

        private static void ResolveErrorCode(Exception exception, out ErrorCode errorCode)
        {
            if (exception == null)
            {
                errorCode = ErrorCode.Success;
                return;
            }
            
            var ofe = exception as OperationFailedException;
            errorCode = ofe != null ? ofe.ErrorCode : ErrorCode.InternalError;
        }
    }
}