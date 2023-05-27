using System.IO;

namespace Uccs.Net
{
	public class DownloadTableRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public int		ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);
				
				var m = Table switch
							  {
									Tables.Accounts		=> core.Database.Accounts.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Authors		=> core.Database.Authors.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Products		=> core.Database.Products.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Realizations	=> core.Database.Realizations.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Releases		=> core.Database.Releases.Clusters.Find(i => i.Id == ClusterId)?.Main,
									_ => throw new RdcEntityException(RdcEntityError.InvalidRequest)
							  };

				if(m == null)
					throw new RdcEntityException(RdcEntityError.ClusterNotFound);
	
				var s = new MemoryStream(m);
				var r = new BinaryReader(s);
	
				s.Position = Offset;
	
				return new DownloadTableResponse{Data = r.ReadBytes((int)Length)};
			}
		}
	}
		
	public class DownloadTableResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
