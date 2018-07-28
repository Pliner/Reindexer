using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reindexer.Logging;

namespace Reindexer.Watcher
{
    public class SingleFileWatcher : IFileWatcher
    {    
        private readonly Subject<FileChangeType> changes = new Subject<FileChangeType>();
        private readonly ILog log = LogProvider.For<DirectoryWatcher>();
        private readonly FileSystemWatcherHandle watcher;
        private readonly string filePath;
        private readonly IFileWatcherDispatcher dispatcher;

        public SingleFileWatcher(string filePath, IFileWatcherDispatcher dispatcher)
        {
            this.filePath = filePath;
            this.dispatcher = dispatcher;

            watcher = FileSystemWatcherUtils.CreateSingleFileWatcher(
                filePath,
                OnPathCreated,
                OnPathChanged,
                OnPathDeleted,
                OnPathRenamed,
                OnError
            );

            Changes = changes.Select(x => new FileChangedEvent(filePath, x));
        }

        private void OnPathRenamed(object sender, RenamedEventArgs e)
        {
            dispatcher.Dispatch(() =>
            {
                if (string.Equals(e.OldFullPath, filePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    log.TraceFormat("File {Path} was deleted", filePath);
                    changes.OnNext(FileChangeType.Deleted);
                }
                else if (string.Equals(e.FullPath, filePath, StringComparison.InvariantCultureIgnoreCase))
                {                
                    log.TraceFormat("File {Path} was created or updated", filePath);
                    changes.OnNext(FileChangeType.CreatedOrUpdated);
                }
            });
        }

        private void OnPathChanged(object sender, FileSystemEventArgs e)
        {
            dispatcher.Dispatch(() => 
            {
                log.TraceFormat("File {Path} was created or updated", filePath);
                changes.OnNext(FileChangeType.CreatedOrUpdated);
            });
        }

        private void OnPathDeleted(object sender, FileSystemEventArgs e)
        {
            dispatcher.Dispatch(() =>
            {
                log.TraceFormat("File {Path} was deleted", filePath);
                changes.OnNext(FileChangeType.Deleted);
            });
        }

        private void OnPathCreated(object sender, FileSystemEventArgs e)
        {
            dispatcher.Dispatch(() =>
            {
                log.TraceFormat("File {Path} was created or updated", filePath);
                changes.OnNext(FileChangeType.CreatedOrUpdated);
            });
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            dispatcher.Dispatch(() =>
            {
                if (File.Exists(filePath))
                {
                    log.TraceFormat("File {Path} was created or updated", filePath);    
                    changes.OnNext(FileChangeType.CreatedOrUpdated);
                }
                else
                {
                    log.TraceFormat("File {Path} was deleted", filePath);
                    changes.OnNext(FileChangeType.Deleted);
                }
            });
        }

        public void Dispose()
        {
            watcher.Dispose();
            changes.Dispose();
        }

        public IObservable<FileChangedEvent> Changes { get; }
        
        public void Start()
        {
            watcher.Start();

            if (!File.Exists(filePath)) return;
            
            log.TraceFormat("File {Path} was created or updated", filePath);    
            changes.OnNext(FileChangeType.CreatedOrUpdated);
        }
    }
}