using System;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class NetCommand : Command
	{
		public const string Keyword = "net";

		public NetCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "peers" :
				{
					var r = Call<PeersReport>(new PeersReportCall {Limit = int.MaxValue}, Workflow);
			
					Dump(	r.Peers, 
							new string[] {"IP", "Status", "PeerRank", "BaseRank", "ChainRank", "SeedRank"}, 
							new Func<PeersReport.Peer, string>[]{	i => i.IP.ToString(),
																	i => i.Status.ToString(),
																	i => i.PeerRank.ToString(),
																	i => i.BaseRank.ToString(),
																	i => i.ChainRank.ToString(),
																	i => i.SeedRank.ToString() });
					return r;
				} 
			
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
