using System;

namespace Reindexer.Watcher
{
    public interface IFileWatcherService : IDisposable
    {
        IObservable<FileChangedEvent> Changes { get; }
        void RegisterFile(string path);
        void RegisterDirectory(string path);
    }
}