using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Reindexer.Indexing;
using Reindexer.Indexing.Quering;
using Xunit;

namespace Reindexer.Tests.Indexing.Quering
{
    public class NotQueryTests
    {
        private static readonly long[] Files = {1, 2, 3, 4, 5};

        [Theory]
        [MemberData(nameof(Execute_Cases))]
        public void Execute(IQuery query, long[] expected)
        {
            var notQuery = new NotQuery(query);
            var index = Substitute.For<IFullTextIndex>();
            index.IndexedDocuments.Returns(Files);
            var actual = notQuery.Execute(index);
            actual.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> Execute_Cases()
        {
            IQuery Q(string evaluationResult)
            {
                var query = Substitute.For<IQuery>();
                query.Execute(Arg.Any<IFullTextIndex>()).Returns(evaluationResult.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray());
                return query;
            }

            object[] T(IQuery query, string expected)
            {
                return new object[] {query, expected.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray()};
            }

            yield return T(Q(""), "1 2 3 4 5");
            yield return T(Q("1"), "2 3 4 5");
            yield return T(Q("3"), "1 2 4 5");
            yield return T(Q("5"), "1 2 3 4");
            yield return T(Q("1 2 3 4 5"), "");
        }
    }
}