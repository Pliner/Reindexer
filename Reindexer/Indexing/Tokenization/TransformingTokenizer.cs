using System;
using System.Collections.Generic;
using System.IO;

namespace Reindexer.Indexing.Tokenization
{
    public class TransformingTokenizer : ITokenizer
    {
        private readonly ITokenizer tokenizer;
        private readonly Func<Token, Token> transformer;

        public TransformingTokenizer(ITokenizer tokenizer, Func<Token, Token> transformer)
        {
            this.tokenizer = tokenizer;
            this.transformer = transformer;
        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            foreach (var token in tokenizer.Tokenize(reader))
                yield return transformer(token);
        }
    }
}