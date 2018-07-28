using System;
using System.Collections.Generic;
using System.IO;

namespace Reindexer.Indexing.Tokenization
{
    public class FilteringTokenizer : ITokenizer
    {
        private readonly ITokenizer tokenizer;
        private readonly Func<Token, bool> filterCondition;

        public FilteringTokenizer(ITokenizer tokenizer, Func<Token, bool> filterCondition)
        {
            this.tokenizer = tokenizer;
            this.filterCondition = filterCondition;
        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            foreach (var token in tokenizer.Tokenize(reader))
                if (!filterCondition(token))
                    yield return token;
        }
    }
}