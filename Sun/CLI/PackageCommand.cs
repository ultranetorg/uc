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

		public PackageCommand(Zone zone, Settings settings, Log log, Func<Core> core, Xon args) : base(zone, settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "update" : 
					return Core.Enqueue(new ResourceUpdation(	GetPrivate("by", "password"), 
																ResourceAddress.Parse(GetString("address")),
																GetHexBytes("hash"),
																ResourceFlags.Package|ResourceFlags.Constant|(Args.Has("analysable") ? ResourceFlags.Analysable : 0)),
										GetAwaitStage(), 
										Workflow);

				case "publish" :
				{
					Core.PackageBase.AddRelease(GetReleaseAddress("address"), 
												Args.Has("previous") ? Version.Parse(GetString("previous")) : null,
												GetString("sources").Split(','), 
												GetString("dependsdirectory"), 
												Workflow);
					return null;
				}

				case "download" :
				{
					var d = Core.PackageBase.Download(GetReleaseAddress("address"), Workflow);

					if(d != null)
					{
						while(!d.Succeeded)
						{
							var r = Core.Filebase.Downloads.Find(i => i.Release == GetResourceAddress("address"));

							Workflow.Log?.Report(this, r != null ? $"{r.File}={r.CompletedLength}/{r.Length} " : null + $"Deps={d.DependenciesRecursiveSuccesses}/{d.DependenciesRecursiveCount}");
							Thread.Sleep(500);
						}
					} 
					else
						Workflow.Log?.Report(this, $"Already downloaded");

					return d;
				}

		   		case "status" :
				{
					try
					{
						var r = Core.Call(Role.Base, i => i.FindResource(GetResourceAddress("address")), Workflow);
	
						Workflow.Log?.Report(this, "   " + r.Resource.Address.ToString());
						Workflow.Log?.Report(this, "   " + r.Resource.Flags);
						Workflow.Log?.Report(this, "   " + Hex.ToHexString(r.Resource.Data));

						return r;
					}
					catch(RdcEntityException ex)
					{
						Workflow.Log?.Report(this, ex.Message);
						return null;
					}
				}
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
