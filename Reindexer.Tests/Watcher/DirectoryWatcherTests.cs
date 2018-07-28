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
    public class DirectoryWatcherTests : IDisposable
    {
        public DirectoryWatcherTests()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
            watcherTempDirectory = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}");
            Directory.CreateDirectory(watcherTempDirectory);
            fileWatcherDispatcher = new FileWatcherDispatcher();
            directoryWatcher = new DirectoryWatcher(watcherTempDirectory, fileWatcherDispatcher);
        }

        public void Dispose()
        {
            directoryWatcher.Dispose();
            fileWatcherDispatcher.Dispose();
            Directory.Delete(tempDirectory, true);
        }

        private const int QuietTimeMs = 1000;

        private readonly string tempDirectory;
        private readonly string watcherTempDirectory;
        private readonly DirectoryWatcher directoryWatcher;
        private readonly FileWatcherDispatcher fileWatcherDispatcher;

        private IEnumerable<FileChangedEvent> CollectEvents(Action action, int expectedEventsCount)
        {
            var are = new CountdownEvent(expectedEventsCount);
            var events = new List<FileChangedEvent>();
            var sampledChanges = directoryWatcher.Changes
                .GroupBy(x => x)
                .Select(x => x.Sample(TimeSpan.FromMilliseconds(QuietTimeMs), ThreadPoolScheduler.Instance))
                .SelectMany(x => x);
            using (sampledChanges.Subscribe(x =>
            {
                events.Add(x);
                are.Signal();
            }))
            {
                directoryWatcher.Start();

                action();

                if (!are.Wait(5000)) throw new TimeoutException();
            }

            return events;
        }

        [Fact]
        public void FileWasCreatedAndDeleted()
        {
            var file = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(file, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Delete(file);
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(file), FileChangedEvent.Deleted(file));
        }

        [Fact]
        public void FileWasCreatedAndRenamed_EscapeFromWatchDirectory()
        {
            var sourceFileName = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");
            var destinationFileName = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(sourceFileName, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Move(sourceFileName, destinationFileName);
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(sourceFileName), FileChangedEvent.Deleted(sourceFileName));
        }

        [Fact]
        public void FileWasCreated()
        {
            var file = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() => File.WriteAllText(file, "Hello"), 1);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(file));
        }

        [Fact]
        public void FileWasCreatedAndRenamed_EntryToWatchDirectory()
        {
            var sourceFileName = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}.txt");
            var destinationFileName = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(sourceFileName, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Move(sourceFileName, destinationFileName);
            }, 1);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(destinationFileName));
        }

        [Fact]
        public void FileWasCreatedAndUpdated()
        {
            var file = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(file, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.WriteAllText(file, "Hello");
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(file), FileChangedEvent.CreatedOrUpdated(file));
        }

        [Fact]
        public void FileWasCreatedAndDirectoryWasRenamed_EntryToWatchDirectory()
        {
            var fileName = $"{Guid.NewGuid():N}.txt";

            var sourceDirectory = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}");
            var sourceFile = Path.Combine(sourceDirectory, fileName);
            Directory.CreateDirectory(sourceDirectory);
            var destinationDirectory = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
            var destinationFile = Path.Combine(destinationDirectory, fileName);

            var events = CollectEvents(() =>
            {
                File.WriteAllText(sourceFile, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                Directory.Move(sourceDirectory, destinationDirectory);
            }, 1);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(destinationFile));
        }

        [Fact]
        public void FileWasCreatedAndDirectoryWasRenamed_EscapeFromWatchDirectory()
        {
            var sourceDirectory = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
            var sourceFile = Path.Combine(sourceDirectory, $"{Guid.NewGuid():N}.txt");
            Directory.CreateDirectory(sourceDirectory);
            var destinationDirectory = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(sourceFile, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                Directory.Move(sourceDirectory, destinationDirectory);
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(sourceFile), FileChangedEvent.Deleted(sourceFile));
        }

        [Fact]
        public void FileWasCreateAndRenamed()
        {
            var sourceFile = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");
            var destinationFile = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(sourceFile, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                File.Move(sourceFile, destinationFile);
            }, 3);

            events.Should().BeEquivalentTo(
                FileChangedEvent.CreatedOrUpdated(sourceFile),
                FileChangedEvent.Deleted(sourceFile),
                FileChangedEvent.CreatedOrUpdated(destinationFile)
            );
        }

        [Fact]
        public void FileWasCreatedAndDirectoryWasDeleted()
        {
            var directory = Path.Combine(watcherTempDirectory, $"{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            var file = Path.Combine(directory, $"{Guid.NewGuid():N}.txt");

            var events = CollectEvents(() =>
            {
                File.WriteAllText(file, "Hello");
                Thread.Sleep(QuietTimeMs * 2);
                Directory.Delete(directory, true);
            }, 2);

            events.Should().BeEquivalentTo(FileChangedEvent.CreatedOrUpdated(file), FileChangedEvent.Deleted(file));
        }
    }
}