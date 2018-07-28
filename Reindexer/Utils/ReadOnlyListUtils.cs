using System;
using System.Collections;
using System.Collections.Generic;

namespace Reindexer.Utils
{
    internal static class ReadOnlyListUtils
    {
        public static IReadOnlyList<T> Append<T>(this IReadOnlyList<T> source, T element)
        {
            if(source == null) throw new ArgumentNullException(nameof(source));
            
            return new AppendList<T>(source, element);
        }
        
        private class AppendList<T> : IReadOnlyList<T>
        {
            private readonly IReadOnlyList<T> head;
            private readonly T tail;

            public AppendList(IReadOnlyList<T> head, T tail)
            {
                this.head = head;
                this.tail = tail;
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var element in head) yield return element;

                yield return tail;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => head.Count + 1;

            public T this[int index]
            {
                get
                {
                    var count = head.Count;
                    if (index < 0) throw new IndexOutOfRangeException();
                    if (index < count) return head[index];
                    if (index == count) return tail;
                    throw new IndexOutOfRangeException();
                }
            }
        }
    }
}