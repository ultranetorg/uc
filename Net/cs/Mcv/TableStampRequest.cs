using System.Linq;

namespace Uccs.Net
{
	public class TableStampRequest : McvCall<TableStampResponse>
	{
		public int		Table { get; set; }
		public byte[]	SuperClusters { get; set; }

		public override PeerResponse Execute()
		{
			if(SuperClusters.Length > TableBase.SuperClustersCountMax)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Mcv.Lock)
			{
				RequireBase();
				
				if(Mcv.BaseState == null)
					throw new NodeException(NodeError.TooEearly);

				if(Table < 0 || Mcv.Tables.Length <= Table)
					throw new RequestException(RequestError.OutOfRange);

				return new TableStampResponse{Clusters = SuperClusters	.SelectMany(s => Mcv.Tables[Table].Clusters
																		.Where(c => c.SuperId == s)
																		.Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
			}
		}
	}
	
	public class TableStampResponse : PeerResponse
	{
		public class Cluster
		{
			public byte[]	Id { get; set; }
			public int		Length { get; set; }
			public byte[]	Hash { get; set; }
		}
	
		public Cluster[]	Clusters { get; set; }
	}
}
