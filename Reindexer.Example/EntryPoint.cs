using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Reindexer.Example
{
    public static class EntryPoint
    {
        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level} {Message:l} {Exception}{NewLine}{Properties}{NewLine}", theme: AnsiConsoleTheme.Grayscale)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateLogger();
            
            var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            using (var reindexer = ReindexerFactory.Create())
            {
                reindexer.RegisterDirectory(Path.Combine(directoryPath, "Data"));
                reindexer.RegisterFile(Path.Combine(directoryPath, "Data", "Dracula.txt"));
                reindexer.RegisterFile(Path.Combine(directoryPath, "Grimms.txt"));
                
                while (true)
                {
                    var query = Console.ReadLine();
                    var foundResults = reindexer.Search(query).ToList();
                    Console.WriteLine("Found {0} results for {1}", foundResults.Count, query);
                    foreach (var foundResult in foundResults)
                    {
                        Console.WriteLine("---> {0}", foundResult);
                    }
                }
            }
        }
    }
}