using System;
using System.IO;
using MultiThreadGzip.Components;

namespace MultiThreadGzip.Helpers
{
    public static class WriteChunkHelper
    {
        public static void WriteCompressedChunk(this Stream stream, Chunk chunk)
        {
            stream.WriteLong(chunk.Position);
            stream.WriteInt(chunk.Length);
            stream.Write(chunk.Data, 0, chunk.Length);
        }

        public static void WriteDecompressedChunk(this Stream stream, Chunk chunk)
        {
            stream.Position = chunk.Position;
            stream.Write(chunk.Data, 0, chunk.Length);
        }
        
        private static void WriteInt(this Stream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(int));
        }
        
        private static void WriteLong(this Stream stream, long value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(long));
        }
    }
}