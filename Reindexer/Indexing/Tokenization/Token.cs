namespace Reindexer.Indexing.Tokenization
{
    public struct Token
    {
        public Token(string payload, string type)
        {
            Payload = payload;
            Type = type;
        }

        public string Payload { get; }

        public string Type { get; }
    }
}