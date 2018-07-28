using System;

namespace Reindexer.Watcher
{
    public interface IFileWatcher : IDisposable
    {
        IObservable<FileChangedEvent> Changes { get; }

        void Start();
    }
}