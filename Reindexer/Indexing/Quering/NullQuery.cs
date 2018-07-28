using System;
using System.Collections.Generic;

namespace Reindexer.Indexing.Quering
{
    public class NullQuery : IQuery
    {
        public static readonly IQuery Instance = new NullQuery();
        
        private NullQuery()
        {
        }

        public IEnumerable<long> Execute(IFullTextIndex index)
        {
            return Array.Empty<long>();
        }

        public IQuery Accept(IQueryVisitor visitor)
        {
            return this;
        }

        public override string ToString() => "";
    }
}