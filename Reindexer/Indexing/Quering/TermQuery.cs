using System;
using System.Collections.Generic;

namespace Reindexer.Indexing.Quering
{
    public class TermQuery : IQuery
    {
        public string Term { get; }

        public TermQuery(string term)
        {
            Term = term ?? throw new ArgumentNullException(nameof(term));
        }

        public IEnumerable<long> Execute(IFullTextIndex index)
        {
            return index.GetMatchingDocuments(Term);
        }

        public IQuery Accept(IQueryVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return Term;
        }
    }
}