using System;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class NetCommand : Command
	{
		public const string Keyword = "net";

		public NetCommand(Program program, Xon args) : base(program, args)
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
					var r = Api<PeersReport>(new PeersReportApc {Limit = int.MaxValue});
			
					Dump(	r.Peers, 
							["IP", "Status", "PeerRank", "BaseRank", "ChainRank", "SeedRank"], 
							[i => i.IP, i => i.Status, i => i.PeerRank, i => i.BaseRank, i => i.ChainRank, i => i.SeedRank]);
					return r;
				} 
			
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
