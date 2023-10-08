using System;
using System.Linq;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class PackageCommand : Command
	{
		public const string Keyword = "package";

		public PackageCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
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
					Sun.PackageHub.AddRelease(PackageAddress.Parse(Args.Nodes[1].Name), 
											Args.Has("previous") ? Version.Parse(GetString("previous")) : null,
											GetString("sources").Split(','), 
											GetString("dependsdirectory"), 
											Workflow);
					return null;
				}

				case "download" :
				{
					var d = Sun.PackageHub.Download(PackageAddress.Parse(Args.Nodes[1].Name), Workflow);

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
