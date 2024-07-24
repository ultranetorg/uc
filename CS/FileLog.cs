using System;
using System.IO;
using System.Linq;

namespace Uccs
{
	public class FileLog
	{
		Log Log;
		int Current;
		int SizeMaximum = 10_000_000;
		int FilesCountMaximum = 10;
		bool ClearOnStart = true;

		public FileLog(Log log, string name, string destination)
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

			Log.Reported += m =>{
									var f = Path.Join(destination, $"{name}.{Current:00000000}.log");
									
									File.AppendAllText(f, m.ToString() + Environment.NewLine);
									
									if(new FileInfo(f).Length > SizeMaximum)
									{
										Current++;

										var fs = Directory.EnumerateFiles(destination, Path.Join($"{name}.*.log"));

										if(fs.Count() > FilesCountMaximum-1)
										{
											File.Delete(fs.Order().First());
										}
									}
								};
		}
	}
}
