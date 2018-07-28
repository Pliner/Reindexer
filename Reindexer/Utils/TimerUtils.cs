using System;
using System.Threading;

namespace Reindexer.Utils
{
    internal static class TimerUtils
    {
        public static IDisposable RunEvery(Action action, TimeSpan period)
        {
            var timerLock = new object();
            var timer = new Timer(_ =>
            {
                if (!Monitor.TryEnter(timerLock)) return;
                
                try
                {
                    action();
                }
                finally
                {
                    Monitor.Exit(timerLock);
                }
            });
            timer.Change(period, period);
            return timer;
        }

        public static void RunOnce(Action action, TimeSpan dueTime)
        {
            var timer = new Timer(s =>
            {
                (s as Timer)?.Dispose();
                action();
            });
            timer.Change(dueTime, Timeout.InfiniteTimeSpan);
        }
    }
}