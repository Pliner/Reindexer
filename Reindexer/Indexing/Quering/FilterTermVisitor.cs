using System;
using System.Collections.Generic;

namespace Reindexer.Indexing.Quering
{
    public class FilterTermVisitor : IQueryVisitor
    {
        private readonly Func<string, bool> filterCondition;

        public FilterTermVisitor(Func<string, bool> filterCondition)
        {
            this.filterCondition = filterCondition;
        }

        public IQuery Visit(TermQuery query)
        {
            return filterCondition(query.Term) ? NullQuery.Instance : new TermQuery(query.Term);
        }

        public IQuery Visit(OrQuery query)
        {
            var modifiedSubqueries = new List<IQuery>();

            foreach (var subQuery in query.Queries)
            {
                var modifiedSubquery = subQuery.Accept(this);
                if (modifiedSubquery == NullQuery.Instance) continue;
   
                modifiedSubqueries.Add(modifiedSubquery);
            }

            switch (modifiedSubqueries.Count)
            {
                case 0:
                    return NullQuery.Instance;
                case 1:
                    return modifiedSubqueries[0];
            }

            return new OrQuery(modifiedSubqueries);
        }

        public IQuery Visit(AndQuery query)
        {
            var modifiedSubqueries = new List<IQuery>();

            foreach (var subQuery in query.Queries)
            {
                var modifiedSubquery = subQuery.Accept(this);
                if (modifiedSubquery == NullQuery.Instance) continue;
   
                modifiedSubqueries.Add(modifiedSubquery);
            }

            switch (modifiedSubqueries.Count)
            {
                case 0:
                    return NullQuery.Instance;
                case 1:
                    return modifiedSubqueries[0];
            }

            return new AndQuery(modifiedSubqueries);
        }

        public IQuery Visit(NotQuery query)
        {
            var modifiedSubquery = query.Query.Accept(this);
            return modifiedSubquery == NullQuery.Instance ? NullQuery.Instance : new NotQuery(modifiedSubquery);
        }
    }
}