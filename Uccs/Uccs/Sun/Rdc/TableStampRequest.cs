using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class TableStampRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public byte[]	SuperClusters { get; set; }

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized)	throw new  RdcNodeException(RdcNodeError.NotSynchronized);
				if(core.Database.BaseState == null)							throw new RdcNodeException(RdcNodeError.TooEearly);

				switch(Table)
				{
					case Tables.Accounts	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Accounts		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Authors		: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Authors		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Products	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Products		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Realizations: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Realizations	.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Releases	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Releases		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
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
