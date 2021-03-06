﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Reindexer.Indexing;
using Reindexer.Indexing.Quering;
using Xunit;

namespace Reindexer.Tests.Indexing.Quering
{
    public class OrQueryTests
    {
        [Theory]
        [MemberData(nameof(Execute_Cases))]
        public void Execute(IQuery[] queries, long[] expected)
        {
            var orQuery = new OrQuery(queries);
            var actual = orQuery.Execute(Substitute.For<IFullTextIndex>());
            actual.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> Execute_Cases()
        {
            IQuery[] Q(params string[] evaluationResult)
            {
                return evaluationResult.Select(x =>
                {
                    var query = Substitute.For<IQuery>();
                    query.Execute(Arg.Any<IFullTextIndex>()).Returns(x.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray());
                    return query;
                }).ToArray();
            }

            object[] T(IQuery[] queries, string expected)
            {
                return new object[] {queries, expected.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray()};
            }

            yield return T(Q(""), "");
            yield return T(Q("", ""), "");
            yield return T(Q("1"), "1");
            yield return T(Q("1 2", "2 3"), "1 2 3");
            yield return T(Q("1 2 3", "2 3 4", "3 4 5"), "1 2 3 4 5");
        }

        [Fact]
        public void Throw_ConstructedWithZeroQueries()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrQuery(Array.Empty<IQuery>()));
        }
    }
}