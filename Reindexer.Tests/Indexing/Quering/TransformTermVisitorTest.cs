using System.Collections.Generic;
using FluentAssertions;
using Reindexer.Indexing.Quering;
using Xunit;

namespace Reindexer.Tests.Indexing.Quering
{
    public class TransformTermVisitorTest
    { 
        private readonly QueryParser queryParser;
        private readonly TransformTermVisitor visitor;

        public TransformTermVisitorTest()
        {
            queryParser = new QueryParser();
            visitor = new TransformTermVisitor(x => x.Substring(1));
        }

        [Theory]
        [MemberData(nameof(TransformTerm_Cases))]
        public void TransformTerm(string input, string parsedQueryString)
        {
            var query = queryParser.ParseQuery(input);
            query.Accept(visitor).ToString().Should().Be(parsedQueryString);
        }

        public static IEnumerable<object[]> TransformTerm_Cases()
        {
            object[] T(string input, string parsedQueryString)
            {
                return new object[] {input, parsedQueryString};
            }

            yield return T("42", "2");
            yield return T("NOT 42", "(NOT 2)");
            yield return T("42 OR 42", "(2 OR 2)");
            yield return T("42 AND 42", "(2 AND 2)");
        }
    }
}