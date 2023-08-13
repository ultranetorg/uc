using System.IO;

namespace Uccs.Net
{
	public class DownloadTableRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public int		ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(!sun.Settings.Roles.HasFlag(Role.Base))	throw new RdcNodeException(RdcNodeError.NotBase);
				
				var m = Table switch
							  {
									Tables.Accounts		=> sun.Mcv.Accounts.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Authors		=> sun.Mcv.Authors.Clusters.Find(i => i.Id == ClusterId)?.Main,
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
