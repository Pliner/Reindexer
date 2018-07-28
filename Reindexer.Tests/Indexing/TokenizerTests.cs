using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Reindexer.Indexing.Tokenization;
using Xunit;

namespace Reindexer.Tests.Indexing
{
    public class TokenizerTests
    {
        private const int TokenMaxLength = 12;
        private readonly Tokenizer tokenizer;

        public TokenizerTests()
        {
            tokenizer = new Tokenizer(TokenMaxLength);
        }

        [Theory]
        [MemberData(nameof(Tokenize_Cases))]
        public void Tokenize(string input, IEnumerable<string> expectedTokens)
        {
            var tokens = tokenizer.Tokenize(new StringReader(input));
            tokens.Select(x => x.Payload).Should().Equal(expectedTokens);
        }

        public static IEnumerable<object[]> Tokenize_Cases()
        {
            object[] T(string input, params string[] tokens)
            {
                return new object[] {input, tokens};
            }

            yield return T("");
            yield return T(" ");
            yield return T("Hello", "Hello");
            yield return T(" Hello", "Hello");
            yield return T("Hello ", "Hello");
            yield return T(" Hello ", "Hello");
            yield return T("42", "42");
            yield return T(new string('x', TokenMaxLength + 1));
        }


        [Fact]
        public void Test()
        {
            var paths = new[]
            {
                "a",
                "a",
                "a/",
                "a/ab",
                "a/a/",
                "a/a",
                "a/b",
                "a/b/",
            };
            
            Array.Sort(paths);
        }
    }
}