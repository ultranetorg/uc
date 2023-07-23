using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class StampRequest : RdcRequest
	{
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized)	throw new  RdcNodeException(RdcNodeError.NotSynchronized);
				if(core.Database.BaseState == null)							throw new RdcNodeException(RdcNodeError.TooEearly);

				var r = new StampResponse {	BaseState				= core.Database.BaseState,
											BaseHash				= core.Database.BaseHash,
											LastCommitedRoundHash	= core.Database.LastCommittedRound.Hash,
											FirstTailRound			= core.Database.Tail.Last().Id,
											LastTailRound			= core.Database.Tail.First().Id,
											Accounts				= core.Database.Accounts.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Authors					= core.Database.Authors.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Resources				= core.Database.Resources.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray()};
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

		public byte[]						BaseState { get; set; }
		public byte[]						BaseHash { get; set; }
		public int							FirstTailRound { get; set; }
		public int							LastTailRound { get; set; }
		public byte[]						LastCommitedRoundHash { get; set; }
		public IEnumerable<SuperCluster>	Accounts { get; set; }
		public IEnumerable<SuperCluster>	Authors { get; set; }
		public IEnumerable<SuperCluster>	Resources { get; set; }
	}
}
