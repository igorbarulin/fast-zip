using System;
using MultiThreadGzip.Components;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    [Category("Unit/ChunkTests")]
    public class ChunkTests
    {
        [Test]
        public void TestCreateChunkBasicFlow()
        {
            Assert.DoesNotThrow(() => new Chunk(0, new byte[] {0}));
        }
        
        [Test]
        public void TestCreateChunkWithPositionLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Chunk(-1, new byte[] {0}));
        }
        
        [Test]
        public void TestCreateChunkWithNullData()
        {
            Assert.Throws<ArgumentNullException>(() => new Chunk(0, null));
        }

        [Test]
        public void TestCreateChunkWithEmptyData()
        {
            Assert.Throws<ArgumentException>(() => new Chunk(0, new byte[0]));
        }
    }
}