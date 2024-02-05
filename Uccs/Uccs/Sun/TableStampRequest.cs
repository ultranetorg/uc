using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class TableStampRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public byte[]	SuperClusters { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			if(SuperClusters.Length > AccountTable.SuperClustersCountMax)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(sun.Lock)
			{
				RequireBase(sun);
				
				if(sun.Mcv.BaseState == null)
					throw new NodeException(NodeError.TooEearly);

				switch(Table)
				{
					case Tables.Accounts : return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => sun.Mcv.Accounts.Clusters.Where(c => c.SuperId == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Authors	 : return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => sun.Mcv.Authors.Clusters.Where(c => c.SuperId == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Analyses : return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => sun.Mcv.Releases.Clusters.Where(c => c.SuperId == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};

					default:
						throw new RequestException(RequestError.IncorrectRequest);
				}
			}
		}
	}
	
	public class TableStampResponse : RdcResponse
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
