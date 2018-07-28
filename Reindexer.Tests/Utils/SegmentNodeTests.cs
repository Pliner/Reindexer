using System;
using FluentAssertions;
using Reindexer.Utils.SegmentTree;
using Xunit;

namespace Reindexer.Tests.Utils
{
    public class SegmentNodeTests
    {
        [Fact]
        public void Test()
        {
            var root = new SegmentTreeNode<int>(StringComparer.InvariantCultureIgnoreCase);

            root.Search(new[] {"a"}, 0).Should().BeEmpty();
            root.Add(new[] {"a", "b", "c"}, 0, 42).Should().BeTrue();
            root.Search(new[] {"a"}, 0).Should().BeEquivalentTo((new[] {"a", "b", "c"}, 42));
            root.Search(new[] {"a", "b"}, 0).Should().BeEquivalentTo((new[] {"a", "b", "c"}, 42));
            root.Search(new[] {"a", "b", "c"}, 0).Should().BeEquivalentTo((new[] {"a", "b", "c"}, 42));
            root.Search(new[] {"a", "b", "c", "d"}, 0).Should().BeEmpty();
            root.Add(new[] {"a", "b"}, 0, 42).Should().BeFalse();
            root.Delete(new[] {"a", "b", "c"}, 0).Should().BeTrue();
            root.Search(new[] {"a"}, 0).Should().BeEmpty();
        }
    }
}