using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiThreadGzip.Components;

namespace MultiThreadGzip.Helpers
{
    public static class ReadChunkHelper
    {
        public static IEnumerable<Chunk> Chunks(this Stream stream, ZipMode zipMode)
        {
            switch (zipMode)
            {
                case ZipMode.Compress:
                    return UncompressedChunks(stream);
                case ZipMode.Decompress:
                    return CompressedChunks(stream);
            }

            return new List<Chunk>(0);
        }
        
        private static IEnumerable<Chunk> UncompressedChunks(this Stream stream)
        {
            const int chunkSize = 8192;
            var buffer = new byte[chunkSize];
            
            var position = 0L;
            var readCount = stream.Read(buffer, 0, chunkSize);
            while (readCount > 0)
            {
                yield return new Chunk(position, buffer.Take(readCount).ToArray());

                position = stream.Position;
                readCount = stream.Read(buffer, 0, chunkSize);
            }
        }
        
        private static IEnumerable<Chunk> CompressedChunks(this Stream stream)
        {
            Chunk chunk;
            while (stream.ReadCompressedChunk(out chunk))
            {
                yield return chunk;
            }
        }
        
        private static bool ReadCompressedChunk(this Stream stream, out Chunk chunk)
        {
            var chunkPosition = 0L;
            var chunkLength = 0;
            if (stream.ReadLong(ref chunkPosition) && stream.ReadInt(ref chunkLength))
            {
                var buffer = new byte[chunkLength];
                if (stream.Read(buffer, 0, chunkLength) == chunkLength)
                {
                    chunk = new Chunk(chunkPosition, buffer);
                    return true;
                }
            }
            
            chunk = null;
            return false;
        }
        
        private static bool ReadInt(this Stream stream, ref int value)
        {
            var buffer = new byte[sizeof(int)];
            var readCount = stream.Read(buffer, 0, sizeof(int));

            if (readCount > 0)
            {
                value = BitConverter.ToInt32(buffer, 0);
                return true;
            }

            return false;
        }

        private static bool ReadLong(this Stream stream, ref long value)
        {
            var buffer = new byte[sizeof(long)];
            var readCount = stream.Read(buffer, 0, sizeof(long));
            
            if (readCount > 0)
            {
                value = BitConverter.ToInt64(buffer, 0);
                return true;
            }

            return false;
        }
    }
}