using System.Collections.Generic;

namespace Reindexer.Indexing.Quering
{
    public interface IQuery
    {
        IEnumerable<long> Execute(IFullTextIndex index);
        
        IQuery Accept(IQueryVisitor visitor);
    }
}