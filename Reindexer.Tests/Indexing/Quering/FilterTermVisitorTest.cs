using System.Collections.Generic;
using FluentAssertions;
using Reindexer.Indexing.Quering;
using Xunit;

namespace Reindexer.Tests.Indexing.Quering
{
    public class FilterTermVisitorTest
    { 
        private readonly QueryParser queryParser;
        private readonly FilterTermVisitor visitor;

        public FilterTermVisitorTest()
        {
            queryParser = new QueryParser();
            visitor = new FilterTermVisitor(x => x == "42");
        }

        [Theory]
        [MemberData(nameof(FilterTerm_Cases))]
        public void FilterTerm(string input, string parsedQueryString)
        {
            var query = queryParser.ParseQuery(input);
            query.Accept(visitor).ToString().Should().Be(parsedQueryString);
        }

        public static IEnumerable<object[]> FilterTerm_Cases()
        {
            object[] T(string input, string parsedQueryString)
            {
                return new object[] {input, parsedQueryString};
            }

            yield return T("42", "");
            yield return T("NOT 42", "");
            yield return T("42 OR 42", "");
            yield return T("42 OR 43 OR 44", "(43 OR 44)");
            yield return T("42 AND 42", "");
            yield return T("42 AND 43", "43");
            yield return T("42 AND 43 AND 44", "(43 AND 44)");
        }
    }
}