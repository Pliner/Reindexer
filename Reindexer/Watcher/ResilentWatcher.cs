using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Reindexer.Logging;
using Reindexer.Utils;

namespace Reindexer.Watcher
{
    public class ResilentWatcher : IFileWatcher
    {
        private readonly Subject<FileChangedEvent> changes = new Subject<FileChangedEvent>();
        private readonly Func<IFileWatcher> createWatcher;
        private readonly ILog log = LogProvider.For<ResilentWatcher>();
        private readonly TimeSpan restartDelay;
        private readonly object watcherLock = new object();
        private volatile IDisposable initializedWatcher;

        public ResilentWatcher(Func<IFileWatcher> createWatcher, TimeSpan restartDelay)
        {
            this.createWatcher = createWatcher;
            this.restartDelay = restartDelay;
        }

        public void Dispose()
        {
            initializedWatcher?.Dispose();
            changes.Dispose();
        }

        public IObservable<FileChangedEvent> Changes => changes;

        public void Start()
        {
            try
            {
                lock (watcherLock)
                {
                    if (initializedWatcher != null) return;

                    var watcher = createWatcher();
                    var watcherChanges = watcher.Changes.Subscribe(changes);
                    watcher.Start();

                    initializedWatcher = Disposable.Create(() =>
                    {
                        watcherChanges.Dispose();
                        watcher.Dispose();
                    });
                }
            }
            catch (Exception exception)
            {
                log.TraceException("Cannot start watcher", exception);

                TimerUtils.RunOnce(Start, restartDelay);
            }
        }
    }
}