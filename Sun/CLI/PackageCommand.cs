using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;
using Uccs.Net;
using System.Threading.Tasks;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class PackageCommand : Command
	{
		public const string Keyword = "package";

		public PackageCommand(Zone zone, Settings settings, Workflow workflow, Func<Net.Sun> sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "build" :
				{
					Sun.Packages.AddRelease(GetReleaseAddress("address"), 
											Args.Has("previous") ? Version.Parse(GetString("previous")) : null,
											GetString("sources").Split(','), 
											GetString("dependsdirectory"), 
											Workflow);
					return null;
				}

				case "download" :
				{
					var d = Sun.Packages.Download(GetReleaseAddress("address"), Workflow);

					void report()
					{
						var f = d.FileDownload;

						Workflow.Log?.Report(this, (f != null ? $"{f.File.Path} = {f.DownloadedLength}/{f.Length}" : null) + $", deps = {d.DependenciesRecursiveSuccesses}/{d.DependenciesRecursiveCount}");
					}

					if(d != null)
					{
						try
						{
							do
							{
								Task.WaitAny(new Task[] {d.Task}, 500, Workflow.Cancellation);
	
								report();
							}
							while(!d.Task.IsCompleted && Workflow.Active);
						}
						catch(OperationCanceledException)
						{
						}
					} 
					else
						Workflow.Log?.Report(this, $"Already downloaded");


					return d;
				}
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
