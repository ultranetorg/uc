using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class StampRequest : RdcRequest
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
				
				if(sun.Mcv.BaseState == null)
					throw new NodeException(NodeError.TooEearly);

				var r = new StampResponse {	BaseState				= sun.Mcv.BaseState,
											BaseHash				= sun.Mcv.BaseHash,
											LastCommitedRoundHash	= sun.Mcv.LastCommittedRound.Hash,
											FirstTailRound			= sun.Mcv.Tail.Last().Id,
											LastTailRound			= sun.Mcv.Tail.First().Id,
											Accounts				= sun.Mcv.Accounts.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Authors					= sun.Mcv.Authors.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Analyses				= sun.Mcv.Releases.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),};
				return r;
			}
		}
	}
	
	public class StampResponse : RdcResponse
	{
		public class SuperCluster
		{
			public byte		Id { get; set; }
			public byte[]	Hash { get; set; }
		}

		public byte[]			BaseState { get; set; }
		public byte[]			BaseHash { get; set; }
		public int				FirstTailRound { get; set; }
		public int				LastTailRound { get; set; }
		public byte[]			LastCommitedRoundHash { get; set; }
		public SuperCluster[]	Accounts { get; set; }
		public SuperCluster[]	Authors { get; set; }
		public SuperCluster[]	Analyses { get; set; }
	}
}
