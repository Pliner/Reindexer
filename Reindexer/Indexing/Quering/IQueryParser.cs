namespace Reindexer.Indexing.Quering
{
    public interface IQueryParser
    {
        IQuery ParseQuery(string query);
    }
}