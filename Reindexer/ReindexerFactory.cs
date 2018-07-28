using System;
using System.Reactive.Disposables;
using Reindexer.Indexing;
using Reindexer.Indexing.Quering;
using Reindexer.Indexing.Tokenization;
using Reindexer.Watcher;

namespace Reindexer
{
    public static class ReindexerFactory
    {
        public static IReindexer Create()
        {
            var tokenizer = new FilteringTokenizer(
                new TransformingTokenizer(new Tokenizer(), x => new Token(English.Stem(x.Payload), x.Type)),
                x => English.IsStopWord(x.Payload)
            );
            var queryParser = new VisitingQueryParser(
                new QueryParser(),
                new FilterTermVisitor(English.IsStopWord),
                new TransformTermVisitor(English.Stem)
            );
            var fullTextIndex = new FullTextIndex();
            var fileWatcherDispatcher = new FileWatcherDispatcher();
            var fileWatcherService = new FileWatcherService(new FileWatcherFactory(fileWatcherDispatcher), TimeSpan.FromSeconds(1));
            return new Reindexer(
                fullTextIndex,
                queryParser,
                tokenizer,
                fileWatcherService,
                Disposable.Create(() =>
                {
                    fileWatcherService.Dispose();
                    fileWatcherDispatcher.Dispose();
                    fullTextIndex.Dispose();
                })
            );
        }
    }
}