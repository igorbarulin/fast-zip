using System;
using System.IO;
using System.Text;
using MultiThreadGzip.Components;

namespace MultiThreadGzip.Helpers
{
    public static class StreamExtensions
    {
        public static void WriteInt(this Stream stream, int value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public static bool ReadInt(this Stream stream, ref int value)
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

        public static void WriteLong(this Stream stream, long value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(long));
        }

        public static bool ReadLong(this Stream stream, ref long value)
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

        public static void WriteSignature(this Stream stream)
        {
            var value = Encoding.ASCII.GetBytes(Constants.Signature);
            stream.Write(value, 0, value.Length);
        }

        public static bool ReadSignature(this Stream stream, ref string signature)
        {
            var length = Encoding.ASCII.GetBytes(Constants.Signature).Length;
            var buffer = new byte[length];
            var readCount = stream.Read(buffer, 0, length);

            if (readCount > 0)
            {
                signature = Encoding.ASCII.GetString(buffer);
                return true;
            }

            return false;
        }
        
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

        public static bool ReadChunk(this Stream stream, out Chunk chunk)
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
    }
}