using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Reindexer.Utils
{
    internal class ConcurrentSet<TElement> : IReadOnlyCollection<TElement>, ICollection<TElement>
    {
        private readonly ConcurrentDictionary<TElement, object> dictionary = new ConcurrentDictionary<TElement, object>();
        
        public IEnumerator<TElement> GetEnumerator()
        {
            return dictionary.Select(x => x.Key).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TElement item)
        {
            dictionary.TryAdd(item, null);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(TElement item)
        {
            return dictionary.ContainsKey(item);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            dictionary.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(TElement item)
        {
            return dictionary.TryRemove(item, out _);
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;
    }
}