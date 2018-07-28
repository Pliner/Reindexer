using System.Threading;

namespace Reindexer.Utils
{
    internal sealed class AtomicLong
    {
        private long value;

        public AtomicLong(long initialValue) => value = initialValue;

        public AtomicLong() : this(0)
        {
        }

        public long Value => Interlocked.Read(ref value);

        public static implicit operator long(AtomicLong atomicLong) => atomicLong.Value;

        public override string ToString() => Value.ToString();

        public long Increment() => Interlocked.Increment(ref value);
    }
}