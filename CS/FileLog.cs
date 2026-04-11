using System.Collections.Concurrent;

namespace Uccs;

public class FileLog
{
	Log						Log;
	int						Current;
	int						SizeMaximum = 10_000_000;
	int						FilesCountMaximum = 10;
	bool					ClearOnStart = true;
	ConcurrentQueue<string> Queue = new ();

	public FileLog(Log log, string name, string destination, Flow flow)
	{
		Log = log;
		
		var fs = Directory.EnumerateFiles(destination, Path.Join($"{name}.*.log"));

		if(ClearOnStart)
		{
			foreach(var i in fs)
				File.Delete(i);

			fs = [];
		}

		Current = fs.Count() == 0 ? 0 : int.Parse(Path.GetFileName(fs.Order().Last()).Split('.')[1]);

		var t = new Thread(() =>	{
										while(flow.Active)
										{
											while(Queue.TryDequeue(out var m))
											{
												var f = Path.Join(destination, $"{name}.{Current:00000000}.log");
	
												File.AppendAllText(f, m + Environment.NewLine);
									
												if(new FileInfo(f).Length > SizeMaximum)
												{
													Current++;
	
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

		Log.Reported += m => {
								Queue.Enqueue(m.ToString());
							 };
	}
}
