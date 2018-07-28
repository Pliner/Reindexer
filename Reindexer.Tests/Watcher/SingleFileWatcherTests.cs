using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using Reindexer.Watcher;
using Xunit;

namespace Reindexer.Tests.Watcher
{
    public class SingleFileWatcherTests : IDisposable
    {
        public SingleFileWatcherTests()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
            watcherTempDirectory = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}");
            filePath = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
            Directory.CreateDirectory(watcherTempDirectory);
            File.WriteAllText(filePath, "Hello");
            dispatcher = new FileWatcherDispatcher();
            singleFileWatcher = new SingleFileWatcher(filePath, dispatcher);
        }

        public void Dispose()
        {
            singleFileWatcher.Dispose();
            dispatcher.Dispose();
            Directory.Delete(tempDirectory, true);
        }

        private const int QuietTimeMs = 1000;

        private readonly string tempDirectory;
        private readonly string watcherTempDirectory;
        private readonly SingleFileWatcher singleFileWatcher;
        private readonly string filePath;
        private readonly FileWatcherDispatcher dispatcher;

        private IEnumerable<FileChangedEvent> CollectEvents(Action action, int expectedEventsCount)
        {
            var are = new CountdownEvent(expectedEventsCount);
            var events = new List<FileChangedEvent>();
            var sampledChanges = singleFileWatcher.Changes 
                .GroupBy(x => x)
                .Select(x => x.Sample(TimeSpan.FromMilliseconds(QuietTimeMs), ThreadPoolScheduler.Instance))
                .SelectMany(x => x);;
            using (sampledChanges.Subscribe(x =>
            {
                events.Add(x);
                are.Signal();
            }))
            {
                singleFileWatcher.Start();

                action();

                if (!are.Wait(5000)) throw new TimeoutException();
            }

            return events;
        }

        [Fact]
        public void FileWasUpdatedAndDeleted()
        {
            var events = CollectEvents(() =>
            {
                File.WriteAllText(filePath, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Delete(filePath);
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(filePath), FileChangedEvent.Deleted(filePath));
        }
        
        [Fact]
        public void NonWatchingFileWasUpdatedAndDeleted()
        {
            var events = CollectEvents(() =>
            {
                var anotherFilePath = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
                File.WriteAllText(anotherFilePath, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Delete(anotherFilePath);
                Thread.Sleep(QuietTimeMs * 2);
            }, 1);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(filePath));
        }
        
        [Fact]
        public void FileWasCreatedAndMoved()
        {
            var events = CollectEvents(() =>
            {
                var anotherFilePath = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
                File.Move(filePath, anotherFilePath);
                Thread.Sleep(QuietTimeMs * 2);
                File.WriteAllText(anotherFilePath, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(filePath), FileChangedEvent.Deleted(filePath));
        }
        
        [Fact]
        public void WatchingFileWasDeletedAndNonWatchingWasCreatedAndRenamedToWatching()
        {
            var events = CollectEvents(() =>
            {   
                File.Delete(filePath);
                var anotherFilePath = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
                File.WriteAllText(anotherFilePath, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Move(anotherFilePath, filePath);
                Thread.Sleep(QuietTimeMs * 2);
            }, 3);

            events.Should().BeEquivalentTo(
                FileChangedEvent.CreatedOrUpdated(filePath),
                FileChangedEvent.Deleted(filePath),
                FileChangedEvent.CreatedOrUpdated(filePath)
                );
        }
    }
}