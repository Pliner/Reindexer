using System;
using System.IO;

namespace Reindexer.Watcher
{
    internal class FileSystemWatcherHandle : IDisposable
    {
        private readonly FileSystemWatcher fsWatcher;
        private readonly Action<FileSystemWatcher> onDispose;

        public FileSystemWatcherHandle(FileSystemWatcher fsWatcher, Action<FileSystemWatcher> onDispose)
        {
            this.fsWatcher = fsWatcher;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose(fsWatcher);
            fsWatcher.Dispose();
        }

        public void Start()
        {
            fsWatcher.EnableRaisingEvents = true;
        }
    }
}