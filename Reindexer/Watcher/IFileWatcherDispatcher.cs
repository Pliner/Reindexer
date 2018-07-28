using System;

namespace Reindexer.Watcher
{
    public interface IFileWatcherDispatcher
    {
        void Dispatch(Action action);
    }
}