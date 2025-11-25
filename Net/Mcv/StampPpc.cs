namespace Uccs.Net;

public class StampPpc : McvPpc<StampPpr>
{
	public override Return Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
			
			if(Mcv.GraphState == null)
				throw new NodeException(NodeError.TooEearly);

			var r = new StampPpr  {GraphState					= Mcv.GraphState,
										GraphHash				= Mcv.GraphHash,
										LastCommitedRoundHash	= Mcv.LastCommitedRound.Hash,
										FirstTailRound			= Mcv.Tail.Last().Id,
										LastTailRound			= Mcv.Tail.First().Id,
										Tables					= Mcv.Tables.Select(i => new StampPpr.Table {Id = i.Id, 
																											 Clusters = i.Clusters.Select(i => new StampPpr.SuperCluster{Id = i.Id, 
																																										 Hash = i.Hash}).ToArray()}).ToArray()};

			return r;
		}
	}
}

public class StampPpr : Return
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
