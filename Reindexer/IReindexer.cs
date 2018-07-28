using System;
using System.Collections.Generic;

namespace Reindexer
{
    public interface IReindexer : IDisposable
    {
        long IndexedFiles { get; }
        void RegisterFile(string path);
        void RegisterDirectory(string path);
        IEnumerable<string> Search(string query);
    }
}