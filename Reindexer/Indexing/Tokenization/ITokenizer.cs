using System.Collections.Generic;
using System.IO;

namespace Reindexer.Indexing.Tokenization
{
    public interface ITokenizer
    {
        IEnumerable<Token> Tokenize(TextReader reader);
    }
}