using MultiThreadGzip.Components;

namespace MultiThreadGzip.Interfaces
{
    public interface ICompressor
    {
        CompressionResult Compress(string originalFileName, string compressedFileName);
        CompressionResult Decompress(string compressedFileName, string decompressedFileName);
    }
}