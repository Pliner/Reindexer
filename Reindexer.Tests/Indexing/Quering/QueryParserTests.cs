using System.Collections.Generic;
using FluentAssertions;
using Reindexer.Indexing.Quering;
using Xunit;

namespace Reindexer.Tests.Indexing.Quering
{
    public class QueryParserTests
    {
        private readonly QueryParser queryParser;

        public QueryParserTests()
        {
            queryParser = new QueryParser();
        }

        [Theory]
        [MemberData(nameof(ParseQuery_Cases))]
        public void ParseQuery(string input, string parsedQueryString)
        {
            var query = queryParser.ParseQuery(input);
            query.ToString().Should().Be(parsedQueryString);
        }

        public static IEnumerable<object[]> ParseQuery_Cases()
        {
            object[] T(string input, string parsedQueryString)
            {
                return new object[] {input, parsedQueryString};
            }

            yield return T("1", "1");
            yield return T("NOT 1", "(NOT 1)");

            yield return T("1 AND 2", "(1 AND 2)");
            yield return T("1 AND 2 AND 3", "(1 AND 2 AND 3)");

            yield return T("1 OR 2", "(1 OR 2)");
            yield return T("1 OR 2 OR 3", "(1 OR 2 OR 3)");
            yield return T("1 OR 2 AND 3", "(1 OR (2 AND 3))");
            yield return T("1 AND 2 OR 3", "((1 AND 2) OR 3)");

            yield return T("1 AND 2 OR 3 AND 4", "((1 AND 2) OR (3 AND 4))");
            yield return T("1 OR 2 AND 3 OR 4", "(1 OR (2 AND 3) OR 4)");

            yield return T("NOT 1 AND 2", "((NOT 1) AND 2)");
            yield return T("1 AND NOT 2", "(1 AND (NOT 2))");

            yield return T("NOT 1 OR 2", "((NOT 1) OR 2)");
            yield return T("1 OR NOT 2", "(1 OR (NOT 2))");
        }
    }
}