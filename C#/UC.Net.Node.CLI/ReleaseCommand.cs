using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Threading;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage: 
	///		   release publish 
	///							by = ACCOUNT 
	///							[password = PASSWORD]
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
					return Send(() => Node.Enqueue(new ReleaseManifest (	GetPrivate("by", "password"), 
																			ReleaseAddress.Parse(GetString("address")),
																			GetString("channel"), 
																			GetVersion("previous"),
																			
																			GetLong("csize"),
																			GetHexBytes("chash"),
																			GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i)),

																			GetVersion("iminimal"),
																			GetLong("isize"),
																			GetHexBytes("ihash"),
																			GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i))
																			)));

				case "publish" :
				{
					Node.Publish(	ReleaseAddress.Parse(GetString("address")), 
									GetString("channel"),
									GetString("sources").Split(','), 
									GetPrivate("by", "password"),
									GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => ReleaseAddress.Parse(i)),
									GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => ReleaseAddress.Parse(i)),
									Flowvizor);

					return null;
				}

				case "download" :
				{
					var d = Node.DownloadPackage(PackageAddress.Parse(GetString("address")), Flowvizor);

					while(!d.Completed)
					{
						Flowvizor.Log?.Report(this, $"{d.CompletedLength + d.Jobs.Sum(i => i.Data != null ? i.Data.Length : 0)}/{d.Length}");
						Thread.Sleep(10);
					}

					return d;
				}

		   		case "status" :
				{
					var r = Node.Connect(Role.Chain, null, Flowvizor).QueryRelease(ReleaseQuery.Parse(GetString("query")), Args.Has("confirmed"));

					foreach(var item in r.Manifests)
					{
						Dump(item);
					}

					return r;
				}
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
