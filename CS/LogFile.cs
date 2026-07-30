using System.Collections.Concurrent;
using System.Text;

namespace Uccs;

public class LogFile : IDisposable
{
    Log                                 Log;
    int                                 Current;
    int                                 SizeMaximum = 10_000_000;
    int                                 FilesCountMaximum = 10;
    bool                                ClearOnStart = true;
    ConcurrentQueue<LogMessage>         Messages;
    string                              DirectoryPath;
    string                              Name;
    string                              PathCurrent;
    int                                 IsProcessing;

    public LogFile(Log log, string name, string destination, Flow flow)
    {
        Name = name;
        DirectoryPath = destination;
        Log = log;

        var fs = Directory.EnumerateFiles(destination, Path.Join($"{name}.*.log"));

        if(ClearOnStart)
        {
            foreach(var i in fs)
                File.Delete(i);

            fs = [];
        }

        Current = fs.Count() == 0 ? 0 : int.Parse(Path.GetFileName(fs.Order().Last()).Split('.')[1]);
        PathCurrent = Path.Join(destination, $"{name}.{Current:00000000}.log");

        Messages = Log.AddListener();

        Log.Reported += OnReported;
    }

    void OnReported(LogMessage message)
    {
        if(Interlocked.CompareExchange(ref IsProcessing, 1, 0) == 0)
        {
            try
            {
                while(Messages.TryDequeue(out var m))
                {
                    File.AppendAllText(PathCurrent, m.ToString());
                    File.AppendAllText(PathCurrent, Environment.NewLine);

                    if(new FileInfo(PathCurrent).Length > SizeMaximum)
                    {
                        Current++;
                        PathCurrent = Path.Join(DirectoryPath, $"{Name}.{Current:00000000}.log");

                        var fs = Directory.EnumerateFiles(DirectoryPath, Path.Join($"{Name}.*.log"));

                        if(fs.Count() > FilesCountMaximum - 1)
                        {
                            File.Delete(fs.Order().First());
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref IsProcessing, 0);
            }

            if(!Messages.IsEmpty)
                OnReported(null);
        }
    }

    public void Dispose() => Log.Reported -= OnReported;
}