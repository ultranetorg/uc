namespace Uccs.Net
{
	public class TableStampRequest : McvPpc<TableStampResponse>
	{
		public int		Table { get; set; }
		public short[]	Clusters { get; set; }

		public override PeerResponse Execute()
		{
			if(Clusters.Length > TableBase.SuperClustersCountMax)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Mcv.Lock)
			{
				RequireBase();
				
				if(Mcv.BaseState == null)
					throw new NodeException(NodeError.TooEearly);

				if(Table < 0 || Mcv.Tables.Length <= Table)
					throw new RequestException(RequestError.OutOfRange);

				return new TableStampResponse {Clusters = Clusters.Select(i =>	{			
																					var c = Mcv.Tables[Table].FindCluster(i);

																					var r = new TableStampResponse.Cluster
																							{
																								Id = c.Id,
																								Buckets = c.Buckets.Select(i => new TableStampResponse.Bucket { Id = i.Id, 
																																								Length = i.MainLength, 
																																								Hash = i.Hash}).ToArray()
																								
																							};
																					return r;
																				})
																				.ToArray()};
				
			}
		}
	}
	
	public class TableStampResponse : PeerResponse
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
			public int		Length { get; set; }
		}
	
		public Cluster[]	Clusters { get; set; }
	}
}
