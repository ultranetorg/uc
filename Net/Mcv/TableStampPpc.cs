namespace Uccs.Net;

public class TableStampPpc : McvPpc<TableStampPpr>
{
	public int		Table { get; set; }
	public short[]	Clusters { get; set; }

	public override Return Execute()
	{
		if(Clusters.Length > TableBase.ClustersCountMax)
			throw new RequestException(RequestError.IncorrectRequest);

		lock(Mcv.Lock)
		{
			RequireGraph();
			
			if(Mcv.GraphState == null)
				throw new NodeException(NodeError.TooEearly);

			if(Table < 0 || Mcv.Tables.Length <= Table)
				throw new RequestException(RequestError.OutOfRange);

			return new TableStampPpr {Clusters = Clusters.Select(i =>	{			
																			var c = Mcv.Tables[Table].FindCluster(i);

																			var r = new TableStampPpr.Cluster
																					{
																						Id = c.Id,
																						Buckets = c.Buckets.Select(i => new TableStampPpr.Bucket  {Id = i.Id, 
																																					//Length = i.Size, 
																																					Hash = i.Hash}).ToArray()
																							
																					};
																			return r;
																		})
																		.ToArray()};
			
		}
	}
}

public class TableStampPpr : Return
{
	public class Cluster
	{
		public short	Id { get; set; }
		public Bucket[]	Buckets { get; set; }
	}

	public class Bucket
	{
		public int		Id { get; set; }
		public byte[]	Hash { get; set; }
		//public int		Length { get; set; }
	}

	public Cluster[]	Clusters { get; set; }
}
