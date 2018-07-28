namespace Reindexer.Indexing.Quering
{
    public interface IQueryVisitor
    {
        IQuery Visit(TermQuery query);
        IQuery Visit(OrQuery query);
        IQuery Visit(AndQuery query);
        IQuery Visit(NotQuery query);
    }
}