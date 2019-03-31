using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MultiThreadGzip.Components;
using MultiThreadGzip.Helpers;
using MultiThreadGzip.Interfaces;
using MultiThreadGzip.Processors;

namespace MultiThreadGzip
{
    public class GzipCompressor : ICompressor
    {
        private const int BufferSize = 4;
        private const int ChunkSize = 8192;
        
        public IProgress Progress
        {
            get { return _progress; }
        }
        
        private readonly Progress _progress = new Progress();

        public CompressionResult Compress(string originalFileName, string compressedFileName)
        {
            var builder = new CompressionResultBuilder(ZipMode.Compress);
            
            _progress.Reset();

            var sw = Stopwatch.StartNew();

            try
            {
                if (!File.Exists(originalFileName))
                {
                    throw new OperationFailedException(ErrorCode.FileDoesNotExist);
                }

                if (InvalidFileName(compressedFileName))
                {
                    throw new OperationFailedException(ErrorCode.InvalidFileName);
                }

                using (var originalFile = new FileStream(originalFileName, FileMode.Open))
                using (var compressedFile = new FileStream(compressedFileName, FileMode.Create))
                {
                    builder.SetInputFileInfo(originalFileName, originalFile.Length);
                    
                    Exception internalException = null;
                    Action<Exception> onException = exception => internalException = exception;
                    
                    compressedFile.WriteSignature();
                
                    using (var writer = CreateChunkWriter(compressedFile, ZipMode.Compress, onException))
                    using (var compressor = CreateCompressor(writer, Environment.ProcessorCount, ZipMode.Compress, onException))
                    {
                        var buffer = new byte[ChunkSize];

                        var position = 0L;
                        var readCount = originalFile.Read(buffer, 0, ChunkSize);
                        while (readCount > 0)
                        {
                            if (internalException != null)
                            {
                                writer.Cancel();
                                compressor.Cancel();
                                
                                throw internalException;
                            }
                            
                            compressor.Enqueue(new Chunk(position, buffer.Take(readCount).ToArray()));
                            position = originalFile.Position;
                            readCount = originalFile.Read(buffer, 0, ChunkSize);
                    
                            _progress.SetProgress(CalculateProgress(originalFile.Position, originalFile.Length));
                            builder.SetOutputFileInfo(compressedFileName, compressedFile.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                builder.SetException(e);
            }
            
            builder.SetLeadTime(sw.Elapsed);
            
            return builder.Result;
        }

        public CompressionResult Decompress(string compressedFileName, string decompressedFileName)
        {
            var builder = new CompressionResultBuilder(ZipMode.Decompress);
            
            _progress.Reset();

            var sw = Stopwatch.StartNew();
            
            try
            {
                if (!File.Exists(compressedFileName))
                {
                    throw new OperationFailedException(ErrorCode.FileDoesNotExist);
                }

                if (InvalidFileName(decompressedFileName))
                {
                    throw new OperationFailedException(ErrorCode.InvalidFileName);
                }
                
                using (var compressedFile = new FileStream(compressedFileName, FileMode.Open))
                using (var decompressedFile = new FileStream(decompressedFileName, FileMode.Create))
                {
                    builder.SetInputFileInfo(compressedFileName, compressedFile.Length);
                    
                    Exception internalException = null;
                    Action<Exception> onException = exception => internalException = exception;

                    var signature = string.Empty;
                    if (compressedFile.ReadSignature(ref signature) == false || signature != Constants.Signature)
                    {
                        throw new OperationFailedException(ErrorCode.NotCompatible);
                    }

                    using (var writer = CreateChunkWriter(decompressedFile, ZipMode.Decompress, onException))
                    using (var decompressor = CreateCompressor(writer, Environment.ProcessorCount, ZipMode.Decompress, onException))
                    {
                        Chunk chunk;
                        while (compressedFile.ReadChunk(out chunk))
                        {
                            if (internalException != null)
                            {
                                writer.Cancel();
                                decompressor.Cancel();
                                
                                throw internalException;
                            }
                            
                            decompressor.Enqueue(chunk);
                    
                            _progress.SetProgress(CalculateProgress(compressedFile.Position, compressedFile.Length));
                            builder.SetOutputFileInfo(decompressedFileName, decompressedFile.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                builder.SetException(e);
            }

            builder.SetLeadTime(sw.Elapsed);

            return builder.Result;
        }
        
        private static bool InvalidFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return true;
            }
            
            var fileName = filePath.Split(Path.DirectorySeparatorChar).LastOrDefault();
            
            return filePath.Any(c => Path.GetInvalidPathChars().Contains(c)) ||
                   string.IsNullOrEmpty(fileName) ||
                   fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c));
        }
        
        private static int CalculateProgress(long position, long length)
        {
            return (int) Math.Ceiling((double) position / length * 100);
        }

        private static IProcessorQueue CreateChunkWriter(Stream stream, ZipMode zipMode, Action<Exception> onException)
        {
            ITaskHandler taskHandler;
            switch (zipMode)
            {
                case ZipMode.Compress:
                    taskHandler = new TaskHandler<Chunk>(stream.WriteCompressedChunk);
                    break;
                case ZipMode.Decompress:
                    taskHandler = new TaskHandler<Chunk>(stream.WriteDecompressedChunk); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => zipMode),
                        zipMode, "Invalid enum value");
            }
            
            return new ProcessorQueue(taskHandler, null, onException, BufferSize);
        }

        private static IProcessorQueue CreateCompressor(IProcessorQueue writer, int threadCount, ZipMode zipMode, Action<Exception> onException)
        {
            ITaskHandler taskHandler;
            switch (zipMode)
            {
                case ZipMode.Compress:
                    taskHandler = new TaskHandler<Chunk, Chunk>(chunk => chunk.Compress());
                    break;
                case ZipMode.Decompress:
                    taskHandler = new TaskHandler<Chunk, Chunk>(chunk => chunk.Decompress());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => zipMode),
                        zipMode, "Invalid enum value");
            }
            
            return new ProcessorQueueSplitter(Enumerable.Range(1, threadCount).Select(i =>
                (IProcessorQueue) new ProcessorQueue(taskHandler, writer, onException, BufferSize)).ToArray());
        }
    }
}