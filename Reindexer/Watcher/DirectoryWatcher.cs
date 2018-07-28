using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Reindexer.Logging;
using Reindexer.Utils;
using Reindexer.Utils.SegmentTree;

namespace Reindexer.Watcher
{
    public class DirectoryWatcher : IFileWatcher
    {
        private static readonly DateTime NonExistingWriteTimeUtc = new DateTime(1601, 1, 1, 1, 1, 1, DateTimeKind.Utc);

        private readonly Subject<FileChangedEvent> changes = new Subject<FileChangedEvent>();
        private readonly string directoryPath;
        private readonly IFileWatcherDispatcher dispatcher;
        private readonly SegmentTreeNode<FileInfo> filesInfo = new SegmentTreeNode<FileInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ILog log = LogProvider.For<DirectoryWatcher>();
        private readonly FileSystemWatcherHandle watcher;

        public DirectoryWatcher(string directoryPath, IFileWatcherDispatcher dispatcher)
        {
            this.directoryPath = directoryPath;
            this.dispatcher = dispatcher;

            watcher = FileSystemWatcherUtils.CreateDirectoryWatcher(
                this.directoryPath,
                OnPathCreated,
                OnPathChanged,
                OnPathDeleted,
                OnPathRenamed,
                OnError
            );
        }

        public void Dispose()
        {
            watcher.Dispose();
            changes.Dispose();
        }

        public void Start()
        {
            watcher.Start();
            var mre = new ManualResetEventSlim(false);
            dispatcher.Dispatch(() =>
            {
                SynchronizeDirectory(directoryPath);
                mre.Set();
            });
            mre.Wait();
        }

        public IObservable<FileChangedEvent> Changes => changes;

        private void OnError(object sender, ErrorEventArgs e)
        {
            dispatcher.Dispatch(() => SynchronizeDirectory(directoryPath));
        }

        private void OnPathRenamed(object sender, RenamedEventArgs e)
        {
            dispatcher.Dispatch(() =>
            {
                Delete(e.OldFullPath);
                CreateOrUpdate(e.FullPath);
            });
        }

        private void OnPathDeleted(object sender, FileSystemEventArgs e)
        {
            dispatcher.Dispatch(() => Delete(e.FullPath));
        }

        private void OnPathChanged(object sender, FileSystemEventArgs e)
        {
            dispatcher.Dispatch(() => CreateOrUpdate(e.FullPath));
        }

        private void OnPathCreated(object sender, FileSystemEventArgs e)
        {
            dispatcher.Dispatch(() => CreateOrUpdate(e.FullPath));
        }

        private void CreateOrUpdate(string path)
        {
            if (File.Exists(path))
            {
                CreateOrUpdateFile(path, path.SplitByDirectorySeparator());
            }
            else if (Directory.Exists(path))
            {
                SynchronizeDirectory(path);
                log.TraceFormat("Directory {Path} was created or updated", path);
            }
            else
            {
                log.TraceFormat("Can't create or update {Path}, because it is not a directory or not a file or not exist", path);
            }
        }

        private void CreateOrUpdateFile(string path, IReadOnlyList<string> pathSegments)
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(path);
            if (lastWriteTime == NonExistingWriteTimeUtc)
            {
                log.TraceFormat("File {Path} is not exist");
                return;
            }

            if (filesInfo.TryGetData(pathSegments, 0, out var fileInfo) && lastWriteTime == fileInfo.LastWriteTime)
            {
                log.TraceFormat("File {Path} was not updated", path);
                return;
            }

            filesInfo.Add(pathSegments, 0, new FileInfo(path, lastWriteTime));
            changes.OnNext(FileChangedEvent.CreatedOrUpdated(path));
            log.TraceFormat("File {Path} was created or updated", path);
        }

        private void Delete(string path)
        {
            var pathSegments = path.SplitByDirectorySeparator();
            if (DeleteFile(path, pathSegments)) return;

            var foundFilesInfo = filesInfo.Search(pathSegments, 0).ToList();
            foreach (var (filePathSegments, fileInfo) in foundFilesInfo)
                DeleteFile(fileInfo.Path, filePathSegments);

            log.TraceFormat("Directory {Path} with {FilesCount} was deleted", path, foundFilesInfo.Count);
        }

        private bool DeleteFile(string path, IReadOnlyList<string> pathSegments)
        {
            if (!filesInfo.Delete(pathSegments, 0)) return false;

            changes.OnNext(FileChangedEvent.Deleted(path));
            log.TraceFormat("File {Path} was deleted", path);
            return true;
        }

        private void SynchronizeDirectory(string targetDirectoryPath)
        {
            IReadOnlyList<string> GetFiles(string path)
            {
                try
                {
                    return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }

            var existingFiles = GetFiles(targetDirectoryPath)
                .Select(x => new {Path = x, Segments = x.SplitByDirectorySeparator()})
                .ToList();

            var targetDirectoryPathsSegments = targetDirectoryPath.SplitByDirectorySeparator();
            var indexedFiles = filesInfo.Search(targetDirectoryPathsSegments, 0).ToList();

            foreach (var existingFilePath in existingFiles)
                CreateOrUpdateFile(existingFilePath.Path, existingFilePath.Segments);

            foreach (var (filePathSegments, fileInfo) in GetMissingFilePaths(indexedFiles, existingFiles.Select(x => x.Segments)))
                DeleteFile(fileInfo.Path, filePathSegments);

            log.TraceFormat("Directory {Path} was synchronized", targetDirectoryPath);
        }

        private static IEnumerable<(IReadOnlyList<string> Segments, T Data)> GetMissingFilePaths<T>(IEnumerable<(IReadOnlyList<string> Segments, T Data)> source, IEnumerable<IReadOnlyList<string>> existing)
        {
            var except = new HashSet<IReadOnlyList<string>>(existing, EqualityComparer.Instance);
            foreach (var element in source)
                if (!except.Contains(element.Segments))
                    yield return element;
        }

        private struct FileInfo
        {
            public FileInfo(string path, DateTime lastWriteTime)
            {
                Path = path;
                LastWriteTime = lastWriteTime;
            }

            public string Path { get; }

            public DateTime LastWriteTime { get; }
        }

        private class EqualityComparer : IEqualityComparer<IReadOnlyList<string>>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer(StringComparer.InvariantCultureIgnoreCase);

            private readonly IEqualityComparer<string> comparer;

            private EqualityComparer(IEqualityComparer<string> comparer)
            {
                this.comparer = comparer;
            }

            public bool Equals(IReadOnlyList<string> first, IReadOnlyList<string> second)
            {
                if (ReferenceEquals(first, second)) return true;
                if (ReferenceEquals(first, null) || ReferenceEquals(second, null)) return false;

                return first.Count == second.Count || first.Zip(second, (x, y) => comparer.Equals(x, y)).All(x => x);
            }

            public int GetHashCode(IReadOnlyList<string> segments)
            {
                return segments.Aggregate(31, (x, y) => x ^ comparer.GetHashCode(y));
            }
        }
    }
}