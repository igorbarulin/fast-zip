using System;
using MultiThreadGzip.Helpers;

namespace MultiThreadGzip.Components
{
    public class Chunk
    {
        public long Position { get; private set; }
        public byte[] Data { get; private set; }
        public int Length { get { return Data.Length; } }
        

        public Chunk(long position, byte[] data)
        {
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException(MemberInfoGetting.GetMemberName(() => position),
                    "Cannot be less than zero");
            }

            if (data == null)
            {
                throw new ArgumentNullException(MemberInfoGetting.GetMemberName(() => data));
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Cannot be empty", MemberInfoGetting.GetMemberName(() => data));
            }
            
            Position = position;
            Data = data;
        }
    }
}