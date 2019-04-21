using System;
using MultiThreadGzip;
using MultiThreadGzip.Components;
using MultiThreadGzip.Helpers;

namespace ConsoleGzip
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Parameters parameters;
            if (Parameters.TryParse(args, out parameters))
            {
                var compressor = new GzipCompressor();

                compressor.Progress.OnProgressChanged += PrintProgress;

                CompressionResult result;

                switch (parameters.ZipMode)
                {
                    case ZipMode.Compress:
                        result = compressor.Compress(parameters.InputFileName, parameters.OutputFileName);
                        break;
                    case ZipMode.Decompress:
                        result = compressor.Decompress(parameters.InputFileName, parameters.OutputFileName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => parameters.ZipMode),
                            parameters.ZipMode, "Invalid enum value");
                }
                
                PrintResult(result);

                if (result.HaveError)
                {
                    Environment.Exit(1);
                }
                
                Environment.Exit(0);
            }
            else
            {
                PrintHelp();
                
                Environment.Exit(1);
            }
        }

        private static void PrintProgress(int progress)
        {
            Console.Write("\rProgress: {0}%", progress);
        }

        private static void PrintResult(CompressionResult result)
        {
            Console.WriteLine();
            
            if (result.HaveError)
            {
                PrintErrorCode(result.ErrorCode, ConsoleColor.Red);
                return;
            }
            
            PrintErrorCode(result.ErrorCode, ConsoleColor.Green);
            PrintSummary(result);
        }

        private static void PrintErrorCode(ErrorCode errorCode, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(GetErrorSummary(errorCode));
            Console.ResetColor();
        }

        private static void PrintSummary(CompressionResult result)
        {
            Console.WriteLine("File {0} ({1} bytes) was {2} to file {3} ({4} bytes) in {5}", 
                result.InputFileName,
                result.InputFileLength, 
                result.ZipMode == ZipMode.Compress ? "compressed" : "decompressed",
                result.OutputFileName, 
                result.OutputFileLength, 
                result.LeadTime);
        }

        private static string GetErrorSummary(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.Success:
                    return "Success";
                case ErrorCode.InternalError:
                    return "Internal error. File may be corrupted";
                case ErrorCode.FileDoesNotExist:
                    return "File does not exist";
                case ErrorCode.InvalidFileName:
                    return "Invalid file name";
                case ErrorCode.NotCompatible:
                    return "File is not compatible";
                default:
                    return string.Empty;
            }
        }

        private static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Usage: compress/decompress inputFile outputFile");
            Console.ResetColor();
        }
    }
}