using System;
using System.Collections.Generic;

namespace Reindexer.Utils.SegmentTree
{
    public class SegmentTreeNode<T>
    {
        private readonly Dictionary<string, SegmentTreeNode<T>> children;
        private readonly Dictionary<string, T> datas;

        public SegmentTreeNode(IEqualityComparer<string> comparer)
        {
            children = new Dictionary<string, SegmentTreeNode<T>>(comparer);
            datas = new Dictionary<string, T>(comparer);
        }

        public bool IsEmpty => children.Count == 0 && datas.Count == 0;

        public bool Add(IReadOnlyList<string> segments, int segmentIndex, T data)
        {
            if (segmentIndex >= segments.Count) throw new ArgumentOutOfRangeException(nameof(segmentIndex));

            var childSegment = segments[segmentIndex];
            if (IsLastSegments(segments, segmentIndex))
            {
                if (children.ContainsKey(childSegment)) return false;
                
                datas[childSegment] = data;
                return true;
            }

            if (datas.ContainsKey(childSegment)) throw new ArgumentOutOfRangeException(nameof(segments));
            if (children.TryGetValue(childSegment, out var child)) return child.Add(segments, segmentIndex + 1, data);

            var newChild = new SegmentTreeNode<T>(children.Comparer);
            children[childSegment] = newChild;
            return newChild.Add(segments, segmentIndex + 1, data);
        }

        public bool TryGetData(IReadOnlyList<string> segments, int segmentIndex, out T data)
        {
            if (segmentIndex >= segments.Count) throw new ArgumentOutOfRangeException(nameof(segmentIndex));

            var childSegment = segments[segmentIndex];
            if (IsLastSegments(segments, segmentIndex)) return datas.TryGetValue(childSegment, out data);
            if (children.TryGetValue(childSegment, out var child)) return child.TryGetData(segments, segmentIndex + 1, out data);

            data = default;
            return false;
        }

        public IEnumerable<(IReadOnlyList<string> Segments, T Data)> Enumerate(IReadOnlyList<string> segments)
        {
            foreach (var kvp in datas)
                yield return (segments.Append(kvp.Key), kvp.Value);

            foreach (var kvp in children)
            {
                var childPrefix = segments.Append(kvp.Key);
                foreach (var element in kvp.Value.Enumerate(childPrefix))
                    yield return element;
            }
        }

        public IEnumerable<(IReadOnlyList<string> Segments, T Data)> Search(IReadOnlyList<string> segments, int segmentIndex)
        {
            if (segmentIndex >= segments.Count) throw new ArgumentOutOfRangeException(nameof(segmentIndex));

            var childSegment = segments[segmentIndex];
            if (IsLastSegments(segments, segmentIndex)) return EnumerateItems(segments, childSegment);
            
            return children.TryGetValue(childSegment, out var child)
                ? child.Search(segments, segmentIndex + 1)
                : Array.Empty<(IReadOnlyList<string>, T)>();
        }

        private IEnumerable<(IReadOnlyList<string> Segments, T Data)> EnumerateItems(IReadOnlyList<string> segments, string childSegment)
        {
            if (datas.TryGetValue(childSegment, out var data))
                yield return (segments, data);
            else if (children.TryGetValue(childSegment, out var child))
            {
                foreach (var item in child.Enumerate(segments))
                    yield return item;
            }
        }

        public bool Delete(IReadOnlyList<string> segments, int segmentIndex)
        {
            if (segmentIndex >= segments.Count) throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex, null);

            var childSegment = segments[segmentIndex];
            if (IsLastSegments(segments, segmentIndex)) return datas.Remove(childSegment);

            if (!children.TryGetValue(childSegment, out var child)) return false;

            var isDeleted = child.Delete(segments, segmentIndex + 1);
            if (child.IsEmpty) children.Remove(childSegment);
            return isDeleted;
        }

        private static bool IsLastSegments(IReadOnlyList<string> segments, int index)
        {
            return index == segments.Count - 1;
        }
    }
}