using System;
using System.IO;
using MultiThreadGzip.Helpers;
using MultiThreadGzip.Interfaces;
using MultiThreadGzip.Processors;

namespace MultiThreadGzip.Components
{
    public class CompressionProcessorFactory : IGzipProcessorFactory
    {
        public IProcessorQueue CreateChunkWriter(Stream stream, Action<Exception> onException, int bufferSize)
        {
            return new ProcessorQueue(new TaskHandler<Chunk>(stream.WriteCompressedChunk), null, onException, bufferSize);
        }

        public IProcessorQueue CreateCompressor(IProcessorQueue writer, int threadCount, Action<Exception> onException, int bufferSize)
        {
            var taskHandler = new TaskHandler<Chunk, Chunk>(chunk => chunk.Compress());
            
            var queues = new IProcessorQueue[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                queues[i] = new ProcessorQueue(taskHandler, writer, onException, bufferSize);
            }
            
            return new ProcessorQueueSplitter(queues);
        }
    }
}