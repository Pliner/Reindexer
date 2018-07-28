namespace Reindexer.Watcher
{
    public interface IFileWatcherFactory
    {
        IFileWatcher CreateDirectoryWatcher(string directoryPath);
        IFileWatcher CreateSingleFileWatcher(string filePath);
    }
}