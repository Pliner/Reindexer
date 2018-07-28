using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Reindexer.Logging;

namespace Reindexer.Watcher
{
    public class FileWatcherDispatcher : IFileWatcherDispatcher, IDisposable
    {
        private readonly BlockingCollection<Action> dispatchQueue = new BlockingCollection<Action>();
        private readonly Task dispatchTask;
        private readonly ILog log = LogProvider.For<FileWatcherDispatcher>();

        public FileWatcherDispatcher()
        {
            dispatchTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var action in dispatchQueue.GetConsumingEnumerable())
                        using (LogProvider.OpenMappedContext("ActionId", Guid.NewGuid()))
                        {
                            try
                            {
                                action();
                            }
                            catch (Exception exception)
                            {
                                log.WarnException(string.Empty, exception);
                            }
                        }
                }
                catch (OperationCanceledException)
                {
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            dispatchQueue.CompleteAdding();
            dispatchTask.Wait();
        }

        public void Dispatch(Action action)
        {
            dispatchQueue.Add(action);
        }
    }
}