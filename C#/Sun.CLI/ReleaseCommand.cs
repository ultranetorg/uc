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
												Args.Has("previous") ? Version.Parse(GetString("previous")) : null,
												GetString("sources").Split(','), 
												GetString("dependsdirectory"), 
												Workflow);
					return null;
				}

				case "download" :
				{
					var d = Core.DownloadRelease(ReleaseAddress.Parse(GetString("address")), Workflow);

					if(d != null)
					{
						while(!d.Successful)
						{
							Workflow.Log?.Report(this, $"{d.CompletedLength}/{d.Length}");
							Thread.Sleep(1000);
						}
					} 
					else
						Workflow.Log?.Report(this, $"Already downloaded");

					return d;
				}

		   		case "status" :
				{
					var r = Core.Connect(Role.Base, null, Workflow).QueryRelease(new []{ReleaseQuery.Parse(GetString("query"))}, Args.Has("confirmed"));

					var i = r.Releases.First();

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
