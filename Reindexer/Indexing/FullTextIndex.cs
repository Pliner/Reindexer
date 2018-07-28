using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reindexer.Utils;

namespace Reindexer.Indexing
{
    public class FullTextIndex : IFullTextIndex, IDisposable
    {
        private readonly AtomicLong lastDocumentId = new AtomicLong();
        private readonly ConcurrentSet<long> deletedDocuments = new ConcurrentSet<long>();
        private readonly ConcurrentSet<long> indexedDocuments = new ConcurrentSet<long>();
        private readonly ConcurrentDictionary<string, ConcurrentSet<long>> termToDocuments = new ConcurrentDictionary<string, ConcurrentSet<long>>(StringComparer.InvariantCultureIgnoreCase);
        private readonly IDisposable timer;

        public FullTextIndex() : this(TimeSpan.FromSeconds(30))
        {
        }

        public FullTextIndex(TimeSpan compactionPeriod)
        {
            timer = TimerUtils.RunEvery(Compact, compactionPeriod);
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        public IReadOnlyCollection<long> IndexedDocuments => indexedDocuments;

        public long IndexTerms(IEnumerable<string> terms)
        {
            var documentId = lastDocumentId.Increment();
            foreach (var term in terms)
            {
                var termDocuments = termToDocuments.GetOrAdd(term, _ => new ConcurrentSet<long>());
                termDocuments.Add(documentId);
                indexedDocuments.Add(documentId);
            }
            return documentId;
        }

        public void DeleteDocument(long documentId)
        {
            deletedDocuments.Add(documentId);
            indexedDocuments.Remove(documentId);
        }

        public IEnumerable<long> GetMatchingDocuments(string term)
        {
            if (!termToDocuments.TryGetValue(term, out var documents)) yield break;

            foreach (var document in documents)
            {
                if (deletedDocuments.Contains(document)) continue;

                yield return document;
            }
        }

        public IReadOnlyCollection<string> IndexedTerms => new ReadOnlyCollection<string>(termToDocuments.Keys);

        private void Compact()
        {
            foreach (var deletedDocument in deletedDocuments)
            {
                foreach (var termDocuments in termToDocuments.Select(x => x.Value)) termDocuments.Remove(deletedDocument);

                deletedDocuments.Remove(deletedDocument);
            }
        }

        private class ReadOnlyCollection<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection<T> collection;

            public ReadOnlyCollection(ICollection<T> collection)
            {
                this.collection = collection;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => collection.Count;
        }
    }
}