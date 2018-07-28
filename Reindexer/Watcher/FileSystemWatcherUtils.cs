using System;
using System.IO;

namespace Reindexer.Watcher
{
    internal static class FileSystemWatcherUtils
    {
        public static FileSystemWatcherHandle CreateDirectoryWatcher(
            string directoryPath,
            FileSystemEventHandler createHandler,
            FileSystemEventHandler updateHandler,
            FileSystemEventHandler deleteHandler,
            RenamedEventHandler renameHandler,
            ErrorEventHandler errorHandler
        )
        {
            var fsWatcher = new FileSystemWatcher(directoryPath, "*.*")
            {
                InternalBufferSize = 64 * 1024,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true
            };

            fsWatcher.Created += createHandler;
            fsWatcher.Changed += updateHandler;
            fsWatcher.Deleted += deleteHandler;
            fsWatcher.Renamed += renameHandler;
            fsWatcher.Error += errorHandler;

            return new FileSystemWatcherHandle(fsWatcher, x =>
            {
                x.EnableRaisingEvents = false;
                x.Error -= errorHandler;
                x.Renamed -= renameHandler;
                x.Deleted -= deleteHandler;
                x.Changed -= updateHandler;
                x.Created -= updateHandler;
            });
        }
        
        public static FileSystemWatcherHandle CreateSingleFileWatcher(
            string filePath,
            FileSystemEventHandler createHandler,
            FileSystemEventHandler updateHandler,
            FileSystemEventHandler deleteHandler,
            RenamedEventHandler renameHandler,
            ErrorEventHandler errorHandler
        )
        {
            var directoryPath = Path.GetDirectoryName(filePath) ?? throw new ArgumentOutOfRangeException(nameof(filePath), filePath, null);
            var fileName = Path.GetFileName(filePath);
            
            var fsWatcher = new FileSystemWatcher(directoryPath, fileName)
            {
                InternalBufferSize = 64 * 1024,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };

            fsWatcher.Created += createHandler;
            fsWatcher.Changed += updateHandler;
            fsWatcher.Deleted += deleteHandler;
            fsWatcher.Renamed += renameHandler;
            fsWatcher.Error += errorHandler;

            return new FileSystemWatcherHandle(fsWatcher, x =>
            {
                x.EnableRaisingEvents = false;
                x.Error -= errorHandler;
                x.Renamed -= renameHandler;
                x.Deleted -= deleteHandler;
                x.Changed -= updateHandler;
                x.Created -= updateHandler;
            });
        }
    }
}