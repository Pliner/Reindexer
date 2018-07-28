using System.Collections.Generic;
using System.IO;

namespace Reindexer.Indexing.Tokenization
{
    public class Tokenizer : ITokenizer
    {
        private readonly int tokenMaxSize;

        public Tokenizer(int tokenMaxSize = 64)
        {
            this.tokenMaxSize = tokenMaxSize;
        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            var buffer = new char[tokenMaxSize];
            var bufferOffset = 0;
            var bufferOverflowed = false;

            for (var readCharacter = reader.Read(); readCharacter != -1; readCharacter = reader.Read())
            {
                var character = (char) readCharacter;
                if (char.IsLetterOrDigit(character))
                {
                    if (bufferOffset >= buffer.Length)
                        bufferOverflowed = true;
                    else
                        buffer[bufferOffset++] = (char) readCharacter;
                }
                else
                {
                    if (bufferOffset > 0 && !bufferOverflowed) yield return CreateToken(buffer, bufferOffset);

                    bufferOffset = 0;
                    bufferOverflowed = false;
                }
            }

            if (bufferOffset > 0 && !bufferOverflowed) yield return CreateToken(buffer, bufferOffset);
        }

        private static Token CreateToken(char[] buffer, int size) => new Token(new string(buffer, 0, size), "word");
    }
}