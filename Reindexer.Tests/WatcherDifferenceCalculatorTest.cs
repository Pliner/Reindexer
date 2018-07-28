using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Reindexer.Watcher;
using Xunit;

namespace Reindexer.Tests
{
    public class WatcherDifferenceCalculatorTest
    {
        [Theory]
        [MemberData(nameof(Calculate_Cases))]
        public void Calculate(string[] directoriesToWatch, string[] filesToWatch, string[] watchingPaths, string[] missing, string[] extra)
        {
            var (actualMissing, actualExtra) = WatcherDifferenceCalculator.Calculate(directoriesToWatch, filesToWatch, watchingPaths);
            actualMissing.Should().BeEquivalentTo(missing);
            actualExtra.Should().BeEquivalentTo(extra);
        }

        public static IEnumerable<object[]> Calculate_Cases()
        {
            string[] S(string input)
            {
                return input.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
            }

            object[] T(string directoriesToWatch, string filesToWatch, string watching, string missing, string extra)
            {
                return new object[] {S(directoriesToWatch), S(filesToWatch), S(watching), S(missing), S(extra)};
            }

            yield return T("/a", "", "", "/a", "");
            yield return T("/a", "", "/a", "", "");
            yield return T("", "", "/a", "", "/a");
            yield return T("", "/a", "", "/a", "");
            yield return T("", "/a", "/a", "", "");
            
            yield return T("/a/aaa", "/a/a", "", "/a/aaa;/a/a", "");
            yield return T("/a/aaa", "/a/a", "/a/aaa;/a/a", "", "");
            yield return T("", "", "/a/aaa;/a/a", "", "/a/aaa;/a/a");

            yield return T("/a", "/a/a/a", "", "/a", "");
        }
    }
}