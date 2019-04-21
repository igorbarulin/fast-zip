using System;
using System.IO;

namespace MultiThreadGzip.Interfaces
{
    public interface IGzipProcessorFactory
    {
        IProcessorQueue CreateChunkWriter(Stream stream, Action<Exception> onException, int bufferSize);

        IProcessorQueue CreateCompressor(IProcessorQueue writer, int threadCount, Action<Exception> onException,
            int bufferSize);
    }
}