using System.IO;
using System.Linq;

namespace MultiThreadGzip.Components
{
    public static class GzipArgumentsValidator
    {
        public static void ThrowBadArguments(string fromFileName, string toFileName)
        {
            ThrowIfFileNotExists(fromFileName);
            ThrowIfInvalidFileName(toFileName);
        }
        
        private static void ThrowIfFileNotExists(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new OperationFailedException(ErrorCode.FileDoesNotExist);
            }
        }

        private static void ThrowIfInvalidFileName(string fileName)
        {
            if (InvalidFileName(fileName))
            {
                throw new OperationFailedException(ErrorCode.InvalidFileName);
            }
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
    }
}