namespace Uccs.Net;

public class StampRequest : McvPpc<StampResponse>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
			
			if(Mcv.GraphState == null)
				throw new NodeException(NodeError.TooEearly);

			var r = new StampResponse  {GraphState				= Mcv.GraphState,
										GraphHash				= Mcv.GraphHash,
										LastCommitedRoundHash	= Mcv.LastCommitedRound.Hash,
										FirstTailRound			= Mcv.Tail.Last().Id,
										LastTailRound			= Mcv.Tail.First().Id,
										Tables					= Mcv.Tables.Select(i => new StampResponse.Table {Id = i.Id, 
																												  Clusters = i.Clusters.Select(i => new StampResponse.SuperCluster {Id = i.Id, 
																																													Hash = i.Hash}).ToArray()}).ToArray()};

			return r;
		}
	}
}

public class StampResponse : PeerResponse
{
	public class SuperCluster
	{
		public short	Id { get; set; }
		public byte[]	Hash { get; set; }
	}

	public class Table
	{
		public int				Id { get; set; }
		public SuperCluster[]	Clusters { get; set; }
	}

	public byte[]		GraphState { get; set; }
	public byte[]		GraphHash { get; set; }
	public int			FirstTailRound { get; set; }
	public int			LastTailRound { get; set; }
	public byte[]		LastCommitedRoundHash { get; set; }
	public Table[]		Tables { get; set; }
}
