using System.Collections.Generic;
using System.Linq;

namespace Reindexer.Indexing.Quering
{
    public class NotQuery : IQuery
    {
        public IQuery Query { get; }

        public NotQuery(IQuery query)
        {
            Query = query;
        }

        public IEnumerable<long> Execute(IFullTextIndex index)
        {
            return index.IndexedDocuments.Except(Query.Execute(index));
        }

        public IQuery Accept(IQueryVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(NOT " + Query + ")";
        }
    }
}