using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Reindexer.Indexing;
using Reindexer.Indexing.Quering;
using Reindexer.Indexing.Tokenization;
using Reindexer.Logging;
using Reindexer.Utils;
using Reindexer.Watcher;

namespace Reindexer
{
    public class Reindexer : IReindexer
    {
        private readonly IFullTextIndex fullTextIndex;
        private readonly IDisposable indexChangesSubscription;
        private readonly ILog log = LogProvider.For<Reindexer>();
        private readonly ConcurrentBidirectionalDictionary<long, string> pathIndex = new ConcurrentBidirectionalDictionary<long, string>(EqualityComparer<long>.Default, StringComparer.InvariantCultureIgnoreCase);
        private readonly IQueryParser queryParser;
        private readonly ITokenizer tokenizer;
        private readonly IFileWatcherService fileWatcherService;
        private readonly IDisposable disposable;

        public Reindexer(
            IFullTextIndex fullTextIndex,
            IQueryParser queryParser,
            ITokenizer tokenizer,
            IFileWatcherService fileWatcherService, 
            IDisposable disposable
        )
        {
            this.fullTextIndex = fullTextIndex;
            this.queryParser = queryParser;
            this.tokenizer = tokenizer;
            this.fileWatcherService = fileWatcherService;
            this.disposable = disposable;

            indexChangesSubscription = fileWatcherService.Changes
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(IndexChanges);
        }

        public long IndexedFiles => fullTextIndex.IndexedDocuments.Count;

        public void RegisterFile(string path)
        {
            fileWatcherService.RegisterFile(path);
        }

        public void RegisterDirectory(string path)
        {
            fileWatcherService.RegisterDirectory(path);
        }

        public IEnumerable<string> Search(string query)
        {
            var parsedQuery = queryParser.ParseQuery(query);
            var paths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var documentId in parsedQuery.Execute(fullTextIndex))
                if (pathIndex.TryGetByFirst(documentId, out var filePath))
                    paths.Add(filePath);

            return paths;
        }

        private void IndexChanges(FileChangedEvent changeEvent)
        {
            switch (changeEvent.Type)
            {
                case FileChangeType.Deleted:
                    IndexDeletedFile(changeEvent.Path);
                    break;
                case FileChangeType.CreatedOrUpdated:
                    IndexNewFile(changeEvent.Path);
                    break;
            }
        }

        private void IndexDeletedFile(string path)
        {
            if (!pathIndex.TryGetBySecond(path, out var id)) return;

            fullTextIndex.DeleteDocument(id);
            pathIndex.TryRemoveBySecond(path, out _);

            log.InfoFormat("File {Path} was removed from index", path);
        }

        private void IndexNewFile(string path)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using (var reader = File.OpenText(path))
                {
                    var terms = tokenizer.Tokenize(reader).Select(x => x.Payload);
                    var id = fullTextIndex.IndexTerms(terms);

                    pathIndex.TryRemoveBySecond(path, out _);
                    pathIndex.Add(id, path);
                }
            }
            catch (Exception exception)
            {
                log.ErrorException("File {Path} was not indexed", exception, path);
            }
            finally
            {
                log.Info("File {Path} was indexed for {Elapsed}", path, stopwatch.Elapsed);
            }
        }

        public void Dispose()
        {
            indexChangesSubscription.Dispose();
            disposable.Dispose();
        }
    }
}