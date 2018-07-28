using System.Linq;

namespace Reindexer.Indexing.Quering
{
    public class VisitingQueryParser : IQueryParser
    {
        private readonly IQueryParser queryParser;
        private readonly IQueryVisitor[] queryVisitors;

        public VisitingQueryParser(IQueryParser queryParser, params IQueryVisitor[] queryVisitors)
        {
            this.queryParser = queryParser;
            this.queryVisitors = queryVisitors;
        }

        public IQuery ParseQuery(string query)
        {
            var parsedQuery = queryParser.ParseQuery(query);
            return queryVisitors.Aggregate(parsedQuery, (x, y) => x.Accept(y));
        }
    }
}