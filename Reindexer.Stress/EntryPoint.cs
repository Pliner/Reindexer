using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Reindexer.Stress
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt", outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level} {Message:l} {Exception}{NewLine}{Properties}{NewLine}")
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level} {Message:l} {Exception}{NewLine}{Properties}{NewLine}", theme: AnsiConsoleTheme.Grayscale, restrictedToMinimumLevel: LogEventLevel.Information)
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .CreateLogger();

            var stressTests = new List<(string, Action)>
            {
                ("Create10FilesAndDeleteOneByOne", () => CreateFilesAndDeleteOneByOne(10, TimeBudget.Start(TimeSpan.FromSeconds(10)))),
                ("Create100FilesAndDeleteOneByOne", () => CreateFilesAndDeleteOneByOne(100, TimeBudget.Start(TimeSpan.FromSeconds(30)))),
                ("Create10FilesAndDeleteDirectory", () => CreateFilesAndDeleteDirectory(10, TimeBudget.Start(TimeSpan.FromSeconds(30)))),
                ("Create100FilesAndDeleteDirectory", () => CreateFilesAndDeleteDirectory(100, TimeBudget.Start(TimeSpan.FromSeconds(30)))),
            };

            foreach (var (name, action) in stressTests)
            {         
                using (LogContext.PushProperty("Test", name))
                {
                    try
                    {
                        action();
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception, "Test was failed");
                    }
                }
            }   
        }
        
        private static void CreateFilesAndDeleteDirectory(int count, TimeBudget timeout)
        {
            var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid().ToString("N")).ToList();

            using (var reindexer = ReindexerFactory.Create())
            {
                reindexer.RegisterDirectory(directoryPath);

                foreach (var id in ids)
                {
                    File.WriteAllText(Path.Combine(directoryPath, id), id);
                }

                Wait(() => reindexer.IndexedFiles == count, timeout);

                foreach (var id in ids)
                {
                    var foundFilenames = reindexer.Search(id)
                        .Select(Path.GetFileName)
                        .ToList();

                    if (foundFilenames.Count == 1 && foundFilenames[0] == id) continue;

                    throw new Exception($"Unexpected result {string.Join(",", foundFilenames)} of searching {id}");
                }
                
                Directory.Delete(directoryPath, true);

                Wait(() => reindexer.IndexedFiles == 0, timeout);

                foreach (var id in ids)
                {
                    var foundFilenames = reindexer.Search(id)
                        .Select(Path.GetFileName)
                        .ToList();

                    if (foundFilenames.Count == 0) continue;

                    throw new Exception($"Unexpected result {string.Join(",", foundFilenames)} of searching {id}");
                }
            }
        }

        
        private static void CreateFilesAndDeleteOneByOne(int count, TimeBudget timeout)
        {
            var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid().ToString("N")).ToList();

            using (var reindexer = ReindexerFactory.Create())
            {
                reindexer.RegisterDirectory(directoryPath);

                foreach (var id in ids)
                {
                    File.WriteAllText(Path.Combine(directoryPath, id), id);
                }

                Wait(() => reindexer.IndexedFiles == count, timeout);

                foreach (var id in ids)
                {
                    var foundFilenames = reindexer.Search(id)
                        .Select(Path.GetFileName)
                        .ToList();

                    if (foundFilenames.Count == 1 && foundFilenames[0] == id) continue;

                    throw new Exception($"Unexpected result {string.Join(",", foundFilenames)} of searching {id}");
                }
                
                foreach (var id in ids)
                {
                    File.Delete(Path.Combine(directoryPath, id));
                }

                Wait(() => reindexer.IndexedFiles == 0, timeout);

                foreach (var id in ids)
                {
                    var foundFilenames = reindexer.Search(id)
                        .Select(Path.GetFileName)
                        .ToList();

                    if (foundFilenames.Count == 0) continue;

                    throw new Exception($"Unexpected result {string.Join(",", foundFilenames)} of searching {id}");
                }
            }
        }

        private static void Wait(Func<bool> condition, TimeBudget timeout)
        {
            while (!timeout.IsExpired)
            {
                if (condition()) return;
                
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            
            throw new TimeoutException();
        }
    }
}