using System;

namespace Reindexer.Watcher
{
    public class FileWatcherFactory : IFileWatcherFactory
    {
        private readonly IFileWatcherDispatcher dispatcher;

        public FileWatcherFactory(IFileWatcherDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public IFileWatcher CreateDirectoryWatcher(string directoryPath)
        {
            return new ResilentWatcher(() => new DirectoryWatcher(directoryPath, dispatcher), TimeSpan.FromSeconds(5));
        }

        public IFileWatcher CreateSingleFileWatcher(string filePath)
        {
            return new ResilentWatcher(() => new SingleFileWatcher(filePath, dispatcher), TimeSpan.FromSeconds(5));
        }
    }
}