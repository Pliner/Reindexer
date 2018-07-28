using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace Reindexer.Indexing.Quering
{
    public class QueryParser : IQueryParser
    {
        private static readonly Parser<IEnumerable<char>> DelimetersParser = Parse.WhiteSpace.Many();

        private static readonly Parser<IQuery> TermParser = Parse.LetterOrDigit.AtLeastOnce().Text().Select(x => new TermQuery(x));

        private static readonly Parser<IQuery> AtomParser = TermParser.Or(Parse.Ref(() => OrParser));

        private static readonly Parser<IQuery> NotParser =
            (from not in Parse.String("NOT")
                from delimeters in DelimetersParser.AtLeastOnce()
                from atom in AtomParser
                select new NotQuery(atom))
            .Or(AtomParser);

        private static readonly Parser<IQuery> AndParser =
            from head in NotParser
            from tail in (
                from leadDelimeters in DelimetersParser.AtLeastOnce()
                from and in Parse.String("AND")
                from trailDelimeters in DelimetersParser.AtLeastOnce()
                from tailPart in Parse.Ref(() => NotParser)
                select tailPart
            ).Many()
            let materializedTail = tail.ToArray()
            select materializedTail.Length == 0
                ? head
                : new AndQuery(new[] {head}.Concat(tail).ToArray());

        private static readonly Parser<IQuery> OrParser =
            from head in AndParser
            from tail in (
                from leadDelimeters in DelimetersParser.AtLeastOnce()
                from or in Parse.String("OR")
                from trailDelimeters in DelimetersParser.AtLeastOnce()
                from tailPart in Parse.Ref(() => AndParser)
                select tailPart
            ).Many()
            let materializedTail = tail.ToArray()
            select materializedTail.Length == 0
                ? head
                : new OrQuery(new[] {head}.Concat(tail).ToArray());

        public IQuery ParseQuery(string query)
        {
            var parsedResult = OrParser(new Input(query));
            return parsedResult.Value;
        }
    }
}