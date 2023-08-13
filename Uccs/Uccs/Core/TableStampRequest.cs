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
			lock(sun.Lock)
			{
				if(sun.Synchronization != Synchronization.Synchronized)	throw new  RdcNodeException(RdcNodeError.NotSynchronized);
				if(sun.Mcv.BaseState == null)							throw new RdcNodeException(RdcNodeError.TooEearly);

				switch(Table)
				{
					case Tables.Accounts	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => sun.Mcv.Accounts.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Authors		: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => sun.Mcv.Authors	.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};

					default:
						throw new RdcEntityException(RdcEntityError.InvalidRequest);
				}
			}
		}
	}
	
	public class TableStampResponse : RdcResponse
	{
		public class Cluster
		{
			public int		Id { get; set; }
			public int		Length { get; set; }
			public byte[]	Hash { get; set; }
		}
	
		public IEnumerable<Cluster>	Clusters { get; set; }
	}
}
