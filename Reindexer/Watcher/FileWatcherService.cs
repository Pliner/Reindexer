using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reindexer.Logging;
using Reindexer.Utils;

namespace Reindexer.Watcher
{
    public class FileWatcherService : IFileWatcherService
    {
        private readonly ILog log = LogProvider.For<FileWatcherService>();
        private readonly Subject<FileChangedEvent> changes = new Subject<FileChangedEvent>();
        private readonly IFileWatcherFactory watcherFactory;

        private readonly Dictionary<string, IDisposable> watchers = new Dictionary<string, IDisposable>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> registeredDirectories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> registeredFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly object registrationLock = new object();

        public FileWatcherService(IFileWatcherFactory watcherFactory, TimeSpan quietPeriod)
        {
            this.watcherFactory = watcherFactory;

            Changes = changes
                .GroupBy(x => x.Path, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => x.Sample(quietPeriod, ThreadPoolScheduler.Instance))
                .SelectMany(x => x);
        }

        public void Dispose()
        {
            foreach (var disposable in watchers.Select(x => x.Value)) disposable.Dispose();

            changes.Dispose();
        }

        public void RegisterFile(string path)
        {
            lock (registrationLock)
            {
                var absolutePath = Path.GetFullPath(path)
                    .ReplaceAlternativeDirectorySeparator()
                    .TrimDirectorySeparatorEnding();

                if (registeredFiles.Contains(absolutePath) || registeredDirectories.Contains(absolutePath))
                    throw new ArgumentOutOfRangeException(nameof(path));

                registeredFiles.Add(absolutePath);

                RegisterInternal();
            }
        }

        public void RegisterDirectory(string path)
        {
            lock (registrationLock)
            {
                var absolutePath = Path.GetFullPath(path)
                    .ReplaceAlternativeDirectorySeparator()
                    .TrimDirectorySeparatorEnding();

                if (registeredFiles.Contains(absolutePath) || registeredDirectories.Contains(absolutePath))
                    throw new ArgumentOutOfRangeException(nameof(path));

                registeredDirectories.Add(absolutePath);

                RegisterInternal();
            }
        }

        public IObservable<FileChangedEvent> Changes { get; }

        private void RegisterInternal()
        {
            var (missingWatchingPaths, staleWatchingPaths) = WatcherDifferenceCalculator.Calculate(registeredDirectories, registeredFiles, watchers.Keys);
            foreach (var staleWatchingPath in staleWatchingPaths)
            {
                if (!watchers.TryGetValue(staleWatchingPath, out var watcher)) continue;

                watcher.Dispose();
                watchers.Remove(staleWatchingPath);
                
                log.TraceFormat("Watcher for {Path} was removed", staleWatchingPath);
            }

            foreach (var missingWatchingPath in missingWatchingPaths)
            {
                var watcher = registeredFiles.Contains(missingWatchingPath)
                    ? watcherFactory.CreateSingleFileWatcher(missingWatchingPath)
                    : watcherFactory.CreateDirectoryWatcher(missingWatchingPath);

                var watcherSubscription = watcher.Changes.Subscribe(x => changes.OnNext(x));
                watcher.Start();
                watchers.Add(missingWatchingPath, Disposable.Create(() =>
                {
                    watcherSubscription.Dispose();
                    watcher.Dispose();
                }));
                
                log.TraceFormat("Watcher for {Path} was added", missingWatchingPath);
            }
        }
    }
}