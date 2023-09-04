using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;
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

					if(d != null)
					{
						while(!d.Succeeded)
						{
							var r = Sun.Resources.FileDownloads.Find(i => i.Resource == GetResourceAddress("address"));

							lock(Sun.Resources.Lock)
								lock(Sun.Packages.Lock)
									Workflow.Log?.Report(this, r != null ? $"{r.File}={r.CompletedLength}/{r.Length} " : null + $"Deps={d.DependenciesRecursiveSuccesses}/{d.DependenciesRecursiveCount}");
							
							Thread.Sleep(500);
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
