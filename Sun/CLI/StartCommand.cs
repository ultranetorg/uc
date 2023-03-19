using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using UC.Net;

namespace UC.Sun.CLI
{
	public class StartCommand : Command
	{
		public const string Keyword = "start";

		public StartCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		string MapPath(VersionAddress r)
		{
			return Path.Join(GetString("products") ?? ProductsDirectory, $"{r.Author}-{r.Product}-{r.Realization}", r.Version.ABCD);
		}


		public override object Execute()
		{
			var r = VersionAddress.Parse(GetString("application"));

			if(!Core.Filebase.ExistsRecursively(r))
			{
				var d = Core.DownloadRelease(r, Workflow);

				while(Core.Downloads.Contains(d))
				{
					Thread.Sleep(100);
				}
			}
			
			Core.Filebase.Unpack(r, GetString("products") ?? ProductsDirectory);
			
			var f = Directory.EnumerateFiles(MapPath(r), "*.start").FirstOrDefault();
			
			if(f != null)
			{
				string setenv(VersionAddress a, string p)
				{
					p += ";" + MapPath(a);

					foreach(var i in Core.Filebase.FindRelease(a).Manifest.CompleteDependencies.Where(i => i.Type == DependencyType.Critical && i.Flags.HasFlag(DependencyFlag.SideBySide)))
					{
						p += ";" + setenv(i.Release, p);
					}

					return p;
				}

				Environment.SetEnvironmentVariable("PATH", setenv(r, Environment.GetEnvironmentVariable("PATH")));
				
				var s = new XonDocument(File.ReadAllText(f));
				
				return Process.Start(s.GetString("Executable"), s.GetString("Arguments"));
			}

			return null;
		}
	}
}
