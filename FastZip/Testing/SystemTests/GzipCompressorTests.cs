using System.Configuration;
using System.IO;
using MultiThreadGzip;
using MultiThreadGzip.Components;
using NUnit.Framework;

namespace SystemTests
{
    [TestFixture]
    public class GzipCompressorTests
    {
        private readonly string _originalFileName;
        private readonly string _compressedFileName;
        private readonly string _decompressedFileName;

        private GzipCompressor _compressor;

        public GzipCompressorTests()
        {
            _originalFileName = ConfigurationManager.AppSettings.Get("OriginalFileName");
            Assume.That(_originalFileName, Is.Not.Null.And.Not.Empty);
            Assume.That(File.Exists(_originalFileName), Is.True);

            _compressedFileName = ConfigurationManager.AppSettings.Get("CompressedFileName");
            Assume.That(_compressedFileName, Is.Not.Null.And.Not.Empty);

            _decompressedFileName = ConfigurationManager.AppSettings.Get("DecompressedFileName");
            Assume.That(_decompressedFileName, Is.Not.Null.And.Not.Empty);
        }

        
        [SetUp]
        public void SetUp()
        {
            _compressor = new GzipCompressor();
        }

        [Test]
        public void TestBasicFlow()
        {
            Assert.That(_compressor.Compress(_originalFileName, _compressedFileName).HaveError, Is.False);
            var result = _compressor.Decompress(_compressedFileName, _decompressedFileName);
            Assert.That(result.HaveError, Is.False);
        }

        [Test]
        public void TestFileDoesNotExist()
        {
            Assert.That(_compressor.Compress("", _compressedFileName).ErrorCode, Is.EqualTo(ErrorCode.FileDoesNotExist));
            Assert.That(_compressor.Decompress("", _decompressedFileName).ErrorCode, Is.EqualTo(ErrorCode.FileDoesNotExist));
        }

        [Test]
        public void TestInvalidFileName()
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                Assert.That(
                    _compressor.Compress(_originalFileName, string.Format("{0}{1}", _compressedFileName, invalidChar))
                        .ErrorCode, Is.EqualTo(ErrorCode.InvalidFileName));
                Assert.That(
                    _compressor.Decompress(_compressedFileName, string.Format("{0}{1}", _decompressedFileName, invalidChar))
                        .ErrorCode, Is.EqualTo(ErrorCode.InvalidFileName));
            }
        }

        [Test]
        public void TestNotCompatible()
        {
            Assert.That(_compressor.Decompress(_originalFileName, _decompressedFileName).ErrorCode,
                Is.EqualTo(ErrorCode.NotCompatible));
        }
    }
}