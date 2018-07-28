using System;
using System.Collections.Generic;
using System.Linq;
using Reindexer.Utils;

namespace Reindexer.Watcher
{
    internal static class WatcherDifferenceCalculator
    {
        public static (IReadOnlyCollection<string>, IReadOnlyCollection<string>) Calculate(
            IReadOnlyCollection<string> directoriesToWatch,
            IReadOnlyCollection<string> filesToToWatch,
            IReadOnlyCollection<string> watchingPaths
        )
        {
            var nonOverlappedDirectories = GetNonOverlappedDirectories(directoriesToWatch);
            var nonOverlappedFiles = GetNonOverlappedFiles(filesToToWatch, nonOverlappedDirectories);
            var selectedPaths = nonOverlappedDirectories.Concat(nonOverlappedFiles)
                .Select(x => x.TrimDirectorySeparatorEnding())
                .ToList();
            var missingPaths = selectedPaths.Except(watchingPaths, StringComparer.InvariantCultureIgnoreCase).ToList();
            var stalePaths = watchingPaths.Except(selectedPaths, StringComparer.InvariantCultureIgnoreCase).ToList();
            return (missingPaths, stalePaths);
        }

        private static IReadOnlyCollection<string> GetNonOverlappedFiles(IReadOnlyCollection<string> files, IReadOnlyCollection<string> directories)
        {
            var nonOverlappedFiles = new HashSet<string>(files, StringComparer.InvariantCultureIgnoreCase);
            foreach (var directory in directories)
            foreach (var file in files)
                if (file.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase))
                    nonOverlappedFiles.Remove(file);
            return nonOverlappedFiles;
        }

        private static List<string> GetNonOverlappedDirectories(IReadOnlyCollection<string> paths)
        {
            var nonOverlappedPaths = new List<string>();
            string lastNonOverlappedPath = null;
            foreach (var path in paths.Select(x => x.EnsureEndsWithDirectorySeparator()).OrderBy(x => x))
                if (lastNonOverlappedPath == null || !path.StartsWith(lastNonOverlappedPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    lastNonOverlappedPath = path;
                    nonOverlappedPaths.Add(path);
                }

            return nonOverlappedPaths;
        }
    }
}