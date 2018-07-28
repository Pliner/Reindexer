using System;
using System.Diagnostics;

namespace Reindexer.Stress
{
    public sealed class TimeBudget
    {
        private readonly TimeSpan precision;
        private readonly Stopwatch watch;

        private TimeBudget(Stopwatch watch, TimeSpan total, TimeSpan precision)
        {
            Total = total;

            this.watch = watch;
            this.precision = precision;
        }

        public TimeSpan Total { get; }

        public TimeSpan Remained
        {
            get
            {
                var remaining = Total - watch.Elapsed;
                return remaining < precision ? TimeSpan.Zero : remaining;
            }
        }

        public bool IsExpired
        {
            get
            {
                var remaining = Total - watch.Elapsed;
                return remaining < precision;
            }
        }

        public static implicit operator TimeSpan(TimeBudget source)
        {
            var remaining = source.Total - source.watch.Elapsed;
            return remaining < source.precision ? TimeSpan.Zero : remaining;
        }

        public static TimeBudget Start(TimeSpan total) => new TimeBudget(Stopwatch.StartNew(), total, TimeSpan.FromMilliseconds(1));
    }
}