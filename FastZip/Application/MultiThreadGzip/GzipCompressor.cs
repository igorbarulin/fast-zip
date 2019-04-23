using System;
using System.Diagnostics;
using System.IO;
using MultiThreadGzip.Components;
using MultiThreadGzip.Helpers;
using MultiThreadGzip.Interfaces;

namespace MultiThreadGzip
{
    public class GzipCompressor : ICompressor
    {
        private const int BufferSize = Constants.BufferSize;

        public IProgress Progress
        {
            get { return _progress; }
        }
        
        private readonly Progress _progress = new Progress();
        
        private CompressionResultBuilder _resultBuilder;
        
        private Stopwatch _stopwatch;
        

        public CompressionResult Compress(string originalFileName, string compressedFileName)
        {
            return InternalProcess(originalFileName, compressedFileName, ZipMode.Compress,
                new CompressionProcessorFactory());
        }

        public CompressionResult Decompress(string compressedFileName, string decompressedFileName)
        {
            return InternalProcess(compressedFileName, decompressedFileName, ZipMode.Decompress,
                new DecompressionProcessorFactory());
        }
        
        private CompressionResult InternalProcess(string fromFileName, string toFileName, ZipMode zipMode, IGzipProcessorFactory processorFactory)
        {
            DropStates();
            
            _resultBuilder.SetZipMode(zipMode);
            
            try
            {
                GzipArgumentsValidator.ThrowBadArguments(fromFileName, toFileName);

                using (var fromFile = new FileStream(fromFileName, FileMode.Open))
                using (var toFile = new FileStream(toFileName, FileMode.Create))
                {
                    _resultBuilder.SetInputFileInfo(fromFileName, fromFile.Length);
                    
                    Exception internalException = null;
                    Action<Exception> onException = exception => internalException = exception;

                    SignatureStage(fromFile, toFile, zipMode);
                    
                    using (var writer = processorFactory.CreateChunkWriter(toFile, onException, BufferSize))
                    using (var compressor = processorFactory.CreateCompressor(writer, Environment.ProcessorCount, onException, BufferSize))
                    {
                        foreach (var chunk in fromFile.Chunks(zipMode))
                        {
                            if (internalException != null)
                            {
                                writer.Cancel();
                                compressor.Cancel();

                                throw internalException;
                            }
                            
                            compressor.Enqueue(chunk);
                            
                            _progress.SetProgress(fromFile.Position, fromFile.Length);
                            _resultBuilder.SetOutputFileInfo(toFileName, toFile.Length);
                        }
                        
                        _progress.SetProgress(100); //to avoid zero progress when original file is empty
                    }
                }
            }
            catch (Exception e)
            {
                _resultBuilder.SetException(e);
            }
            
            _resultBuilder.SetLeadTime(_stopwatch.Elapsed);
            
            return _resultBuilder.Result;
        }
        
        private void DropStates()
        {
            _progress.Reset();
            _resultBuilder = new CompressionResultBuilder();
            _stopwatch = Stopwatch.StartNew();
        }

        private static void SignatureStage(Stream fromFile, Stream toFile, ZipMode zipMode)
        {
            switch (zipMode)
            {
                case ZipMode.Compress:
                    toFile.WriteSignature();
                    break;
                case ZipMode.Decompress:
                    fromFile.ThrowBadSignature();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => zipMode),
                        zipMode, "Invalid enum value");
            }
        }
    }
}