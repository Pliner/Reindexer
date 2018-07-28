using System;
using System.Collections.Generic;
using System.IO;

namespace Reindexer.Utils
{
    internal static class StringUtils
    {
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        public static string EnsureEndsWithDirectorySeparator(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
 
            return source.EndsWithDirectorySeparator() ? source : string.Concat(source, DirectorySeparator);
        }

        public static string TrimDirectorySeparatorEnding(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.EndsWithDirectorySeparator() ? source.Substring(0, source.Length - 1) : source;
        }

        public static bool EndsWithDirectorySeparator(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.EndsWith(DirectorySeparator);
        }

        public static string ReplaceAlternativeDirectorySeparator(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static IReadOnlyList<string> SplitByDirectorySeparator(this string source)
        {
            return source.Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}