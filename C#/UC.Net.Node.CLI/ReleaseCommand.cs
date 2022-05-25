using System;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

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

				case "publish" : 
					return Send(() => Client.Enqueue(new ReleaseManifest (	GetPrivate("by", "password"), 
																			ReleaseAddress.Parse(GetString("address")),
																			GetString("channel"), 
																			Version.Parse(GetString("previous")),
																			Version.Parse(GetString("minimal")),
																			GetStringOrEmpty("cdependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i)).ToList(),
																			GetStringOrEmpty("idependencies").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i =>  ReleaseAddress.Parse(i)).ToList()
																			)));

		   		case "status" :
				{
					var r = Client.QueryRelease(new []{ReleaseQuery.Parse(GetString("query"))}, Args.Has("confirmed"));

					foreach(var item in r)
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
