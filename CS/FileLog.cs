using System.Collections.Concurrent;
using System.Text;

namespace Uccs;

public class FileLog
{
	Log								Log;
	int								Current;
	int								SizeMaximum = 10_000_000;
	int								FilesCountMaximum = 10;
	bool							ClearOnStart = true;
	ConcurrentQueue<LogMessage>		Messages;
	string							DirectoryPath;
	string							Name;
	object							Lock = new();

	public FileLog(Log log, string name, string destination, Flow flow)
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

		Messages = Log.AddListener();

		var path = Path.Join(destination, $"{name}.{Current:00000000}.log");

		var t = new Thread(() =>	{
										while(flow.Active)
										{
											while(Messages.TryDequeue(out var m))
											{
												File.AppendAllText(path, m.ToString());
												File.AppendAllText(path, Environment.NewLine);
									
												if(new FileInfo(path).Length > SizeMaximum)
												{
													Current++;
													path = Path.Join(destination, $"{name}.{Current:00000000}.log");
	
													var fs = Directory.EnumerateFiles(destination, Path.Join($"{name}.*.log"));
	
													if(fs.Count() > FilesCountMaximum-1)
													{
														File.Delete(fs.Order().First());
													}
												}
											}
	
											Thread.Sleep(1000);
										}
									});
		t.Name = name + " Log";
		t.Start();
	}
}
