using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class StartCommand : Command
	{
		public const string Keyword = "start";

		public StartCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		string MapPath(ReleaseAddress r)
		{
			return Path.Join(Settings.ProductsPath, $"{r.Author}-{r.Product}-{r.Realization}", r.Version.ABC);
		}

		public override object Execute()
		{
			var r = ReleaseAddress.Parse(GetString("application"));

			if(!Core.Filebase.ExistsRecursively(r))
			{
				var d = Core.DownloadRelease(r, Workflow);

				while(Core.Downloads.Contains(d))
				{
					Thread.Sleep(100);
				}
			}
			
			Core.Filebase.Unpack(r);
			
			var f = Directory.EnumerateFiles(MapPath(r), "*.start").FirstOrDefault();
			
			if(f != null)
			{
				string setenv(ReleaseAddress a, string p)
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
