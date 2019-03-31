using ConsoleGzip;
using MultiThreadGzip;
using MultiThreadGzip.Components;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    [Category("Unit/ParametersTests")]
    public class ParametersTests
    {
        [TestCase(new[] {"compress", "file1", "file2"}, ZipMode.Compress)]
        [TestCase(new[] {"COMPRESS", "file1", "file2"}, ZipMode.Compress)]
        [TestCase(new[] {"decompress", "file1", "file2"}, ZipMode.Decompress)]
        [TestCase(new[] {"DECOMPRESS", "file1", "file2"}, ZipMode.Decompress)]
        public void TestParseArgumentsBasicFlow(string[] args, ZipMode expectedZipMode)
        {
            Parameters parameters;
            Assert.That(Parameters.TryParse(args, out parameters), Is.True);
            
            Assert.That(parameters.ZipMode, Is.EqualTo(expectedZipMode));
            Assert.That(parameters.InputFileName, Is.EqualTo(args[1]));
            Assert.That(parameters.OutputFileName, Is.EqualTo(args[2]));
        }

        [TestCase("")]
        [TestCase("compress")]
        [TestCase("compress", "file1")]
        public void TestParseArgumentsFail(params string[] args)
        {
            Parameters parameters;
            Assert.That(Parameters.TryParse(args, out parameters), Is.False);
        }
    }
}