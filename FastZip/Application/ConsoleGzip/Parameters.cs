using System;
using MultiThreadGzip;
using MultiThreadGzip.Components;

namespace ConsoleGzip
{
    public class Parameters
    {
        public ZipMode ZipMode { get; private set; }
        public string InputFileName { get; private set; }
        public string OutputFileName { get; private set; }

        public Parameters(ZipMode zipMode, string inputFileName, string outputFileName)
        {
            ZipMode = zipMode;
            InputFileName = inputFileName;
            OutputFileName = outputFileName;
        }

        public static bool TryParse(string[] args, out Parameters parameters)
        {
            ZipMode zipMode;
            string inputFileName;
            string outputFileName;
            
            try
            {
                zipMode = (ZipMode) Enum.Parse(typeof(ZipMode), args[0], true);
                inputFileName = args[1];
                outputFileName = args[2];
            }
            catch (Exception)
            {
                parameters = null;
                return false;
            }

            parameters = new Parameters(zipMode, inputFileName, outputFileName);
            return true;
        }
    }
}