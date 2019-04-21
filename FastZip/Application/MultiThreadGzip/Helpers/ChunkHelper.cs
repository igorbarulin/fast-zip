using System.IO;
using System.IO.Compression;
using MultiThreadGzip.Components;

namespace MultiThreadGzip.Helpers
{
    public static class ChunkHelper
    {
        public static Chunk Compress(this Chunk chunk)
        {
            using (var mem = new MemoryStream())
            {
                using (var gzip = new GZipStream(mem, CompressionMode.Compress))
                {
                    gzip.Write(chunk.Data, 0, chunk.Length);
                }
                    
                return new Chunk(chunk.Position, mem.ToArray());
            }
        }

        public static Chunk Decompress(this Chunk chunk)
        {
            const int bufferSize = 512;
            
            var buffer = new byte[bufferSize];
            using (var mem = new MemoryStream())
            using (var gzip = new GZipStream(new MemoryStream(chunk.Data), CompressionMode.Decompress))
            {
                var readCount = gzip.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    mem.Write(buffer, 0, readCount);
                    readCount = gzip.Read(buffer, 0, bufferSize);
                }
                
                return new Chunk(chunk.Position, mem.ToArray());
            }
        }
    }
}