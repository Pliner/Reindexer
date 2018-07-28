using System.Collections.Generic;

namespace Reindexer.Indexing
{
    public interface IFullTextIndex
    {
        IReadOnlyCollection<long> IndexedDocuments { get; }

        long IndexTerms(IEnumerable<string> terms);

        void DeleteDocument(long documentId);

        IEnumerable<long> GetMatchingDocuments(string term);
        
        IReadOnlyCollection<string> IndexedTerms { get; }
    }
}