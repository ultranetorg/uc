using System;
using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class StampRequest : RdcCall<StampResponse>
	{
		public override RdcResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
				
				if(Mcv.BaseState == null)
					throw new NodeException(NodeError.TooEearly);

				var r = new StampResponse {	BaseState				= Mcv.BaseState,
											BaseHash				= Mcv.BaseHash,
											LastCommitedRoundHash	= Mcv.LastCommittedRound.Hash,
											FirstTailRound			= Mcv.Tail.Last().Id,
											LastTailRound			= Mcv.Tail.First().Id,
											Tables					= Mcv.Tables.Select(i => new StampResponse.Table {	Id = i.Id, 
																														SuperClusters =	i.SuperClusters.Select(i => new StampResponse.SuperCluster{	Id = i.Key, 
																																																	Hash = i.Value}).ToArray()}).ToArray()};

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

		public class Table
		{
			public int				Id { get; set; }
			public SuperCluster[]	SuperClusters { get; set; }
		}

		public byte[]		BaseState { get; set; }
		public byte[]		BaseHash { get; set; }
		public int			FirstTailRound { get; set; }
		public int			LastTailRound { get; set; }
		public byte[]		LastCommitedRoundHash { get; set; }
		public Table[]		Tables { get; set; }
	}
}
