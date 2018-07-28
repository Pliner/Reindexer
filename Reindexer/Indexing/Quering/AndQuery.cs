using System;
using System.Collections.Generic;
using System.Linq;

namespace Reindexer.Indexing.Quering
{
    public class AndQuery : IQuery
    {
        public IReadOnlyCollection<IQuery> Queries { get; }

        public AndQuery(IReadOnlyCollection<IQuery> queries)
        {
            if (queries.Count == 0) throw new ArgumentOutOfRangeException(nameof(queries.Count), queries.Count, null);

            Queries = queries;
        }

        public IEnumerable<long> Execute(IFullTextIndex index)
        {
            var queryEvaluations = new Queue<IEnumerable<long>>();
            foreach (var query in Queries)
            {
                var queryEvaluation = query.Execute(index);
                queryEvaluations.Enqueue(queryEvaluation);
            }

            while (queryEvaluations.Count > 1)
            {
                var first = queryEvaluations.Dequeue();
                var second = queryEvaluations.Dequeue();
                queryEvaluations.Enqueue(first.Intersect(second));
            }

            return queryEvaluations.Dequeue();
        }

        public IQuery Accept(IQueryVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + string.Join(" AND ", Queries.Select(x => x.ToString())) + ")";
        }
    }
}