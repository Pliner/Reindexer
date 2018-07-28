using System;
using System.Collections.Generic;

namespace Reindexer.Utils
{
    internal class ConcurrentBidirectionalDictionary<TFirst, TSecond>
    {
        private readonly Dictionary<TFirst, TSecond> firstToSecond;
        private readonly object modificationLock = new object();
        private readonly Dictionary<TSecond, TFirst> secondToFirst;

        public ConcurrentBidirectionalDictionary(IEqualityComparer<TFirst> first, IEqualityComparer<TSecond> second)
        {
            firstToSecond = new Dictionary<TFirst, TSecond>(first);
            secondToFirst = new Dictionary<TSecond, TFirst>(second);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            lock (modificationLock)
            {
                return firstToSecond.TryGetValue(first, out second);
            }
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            lock (modificationLock)
            {
                return secondToFirst.TryGetValue(second, out first);
            }
        }

        public void Add(TFirst first, TSecond second)
        {
            lock (modificationLock)
            {
                if (firstToSecond.ContainsKey(first)) throw new ArgumentException();
                if (secondToFirst.ContainsKey(second)) throw new ArgumentException();

                firstToSecond.Add(first, second);
                secondToFirst.Add(second, first);
            }
        }

        public bool TryRemoveBySecond(TSecond second, out TFirst first)
        {
            lock (modificationLock)
            {
                if (!secondToFirst.TryGetValue(second, out first)) return false;

                firstToSecond.Remove(first);
                secondToFirst.Remove(second);
                return true;
            }
        }
    }
}