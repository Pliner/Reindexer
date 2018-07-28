using System;
using System.Collections.Generic;

namespace Reindexer.Indexing.Tokenization
{
    public static class English
    {
        public static readonly HashSet<string> StopWords = new HashSet<string>(
            new[]
            {
                "a", "an", "and", "are", "as", "at", "be", "but", "by", "for",
                "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the",
                "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"
            },
            StringComparer.InvariantCultureIgnoreCase
        );

        public static bool IsStopWord(string word)
        {
            return StopWords.Contains(word);
        }

        public static string Stem(string source)
        {
            var size = source.Length;
            
            if (size < 3 || source[size - 1] != 's') return source;

            switch (source[size - 2])
            {
                case 'u':
                case 's':
                    return source;
                case 'e':
                    if (size > 3 && source[size - 3] == 'i' && source[size - 4] != 'a' && source[size - 4] != 'e')
                        return string.Concat(source.Substring(0, size - 3), 'y'.ToString());

                    if (source[size - 3] == 'i' || source[size - 3] == 'a' || source[size - 3] == 'o' || source[size - 3] == 'e')
                        return source;
                    
                    break;
            }

            return source.Substring(size - 1);
        }
    }
}