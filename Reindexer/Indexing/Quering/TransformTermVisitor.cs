using System;
using System.Collections.Generic;

namespace Reindexer.Indexing.Quering
{
    public class TransformTermVisitor : IQueryVisitor
    {
        private readonly Func<string, string> transformer;

        public TransformTermVisitor(Func<string, string> transformer)
        {
            this.transformer = transformer;
        }

        public IQuery Visit(TermQuery query)
        {
            return new TermQuery(transformer(query.Term));
        }

        public IQuery Visit(OrQuery query)
        {
            var modifiedSubqueries = new List<IQuery>();
            foreach (var subQuery in query.Queries)
                modifiedSubqueries.Add(subQuery.Accept(this));
            return new OrQuery(modifiedSubqueries);
        }

        public IQuery Visit(AndQuery query)
        {
            var modifiedSubqueries = new List<IQuery>();
            foreach (var subQuery in query.Queries)
                modifiedSubqueries.Add(subQuery.Accept(this));
            return new AndQuery(modifiedSubqueries);
        }

        public IQuery Visit(NotQuery query)
        {
            return new NotQuery(query.Query.Accept(this));
        }
    }
}