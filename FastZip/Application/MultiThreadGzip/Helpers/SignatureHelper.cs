using System.IO;
using System.Text;
using MultiThreadGzip.Components;

namespace MultiThreadGzip.Helpers
{
    public static class SignatureHelper
    {
        public static void WriteSignature(this Stream stream)
        {
            var value = Encoding.ASCII.GetBytes(Constants.Signature);
            stream.Write(value, 0, value.Length);
        }

        public static void ThrowBadSignature(this Stream stream)
        {
            var signature = string.Empty;
            if (stream.ReadSignature(ref signature) == false || signature != Constants.Signature)
            {
                throw new OperationFailedException(ErrorCode.NotCompatible);
            }
        }

        private static bool ReadSignature(this Stream stream, ref string signature)
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
    }
}