namespace Reindexer.Watcher
{
    public struct FileChangedEvent
    {
        public static FileChangedEvent CreatedOrUpdated(string path)
        {
            return new FileChangedEvent(path, FileChangeType.CreatedOrUpdated);
        }

        public static FileChangedEvent Deleted(string path)
        {
            return new FileChangedEvent(path, FileChangeType.Deleted);
        }
        
        public FileChangedEvent(string path, FileChangeType type)
        {
            Path = path;
            Type = type;
        }

        public bool Equals(FileChangedEvent other)
        {
            return string.Equals(Path, other.Path) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FileChangedEvent @event && Equals(@event);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Path.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }

        public string Path { get; }
        public FileChangeType Type { get; }
    }
}