using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;
using UC.Net;

namespace UC.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		   release publish 
	/// </summary>
	public class ReleaseCommand : Command
	{
		public const string Keyword = "release";

		public ReleaseCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{

				case "declare" : 
					return Core.Enqueue(new ReleaseRegistration(GetPrivate("by", "password"), 
																ReleaseAddress.Parse(GetString("address")),
																GetString("channel"),
																GetHexBytes("hash")),
										GetAwaitStage(), 
										Workflow);

				case "add" :
				{
					Core.Filebase.AddRelease(	ReleaseAddress.Parse(GetString("address")), 
												//GetString("channel"),
												GetString("sources").Split(','), 
												GetString("dependsdirectory"), 
												//GetPrivate("by", "password"),
												//GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => ReleaseAddress.Parse(i)),
												//GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => ReleaseAddress.Parse(i)),
												//GetAwaitStage(),
												Workflow);

					return null;
				}

				case "download" :
				{
					var d = Core.DownloadRelease(ReleaseAddress.Parse(GetString("address")), Workflow);

					while(!d.Completed)
					{
						Workflow.Log?.Report(this, $"{d.CompletedLength + d.Jobs.Sum(i => i.Data != null ? i.Data.Length : 0)}/{d.Length}");
						Thread.Sleep(1000);
					}

					return d;
				}

		   		case "status" :
				{
					var r = Core.Connect(Role.Chain, null, Workflow).QueryRelease(new []{ReleaseQuery.Parse(GetString("query"))}, Args.Has("confirmed"));

					var i = r.Results.First();

					if(i != null)
					{
						Workflow.Log?.Report(this, null, "   " + i.Registration.Release.ToString());
						Workflow.Log?.Report(this, null, "   " + i.Registration.Channel);
						Workflow.Log?.Report(this, null, "   " + Hex.ToHexString(i.Registration.Manifest));
					}
					else
						Workflow.Log?.Report(this, null, "Not found");

					return r;
				}
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
