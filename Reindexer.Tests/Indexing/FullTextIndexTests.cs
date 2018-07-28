using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using Reindexer.Indexing;
using Xunit;

namespace Reindexer.Tests.Indexing
{
    public class FullTextIndexTests : IDisposable
    {
        private readonly FullTextIndex index;

        public FullTextIndexTests()
        {
            index = new FullTextIndex(1.Seconds());
        }

        [Fact]
        public void FindTerm_TermAndDocumentAdded()
        {
            var documentId = index.IndexTerms(new[] {"Term"});

            index.IndexedDocuments.Should().BeEquivalentTo(documentId);
            index.GetMatchingDocuments("Term").Should().BeEquivalentTo(documentId);
        }

        [Fact]
        public void NotFindTerm()
        {
            index.IndexedDocuments.Should().BeEquivalentTo();
            index.GetMatchingDocuments("Term").Should().BeEquivalentTo();
        }

        [Fact]
        public void NotFindTerm_DocumentDeleted()
        {
            var documentId = index.IndexTerms(new[] {"Term"});
            index.DeleteDocument(documentId);
            
            index.IndexedDocuments.Should().BeEquivalentTo();
            index.GetMatchingDocuments("Term").Should().BeEquivalentTo();
        }

        public void Dispose()
        {
            index.Dispose();
        }
    }
}