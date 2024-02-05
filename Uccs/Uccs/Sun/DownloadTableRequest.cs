using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class DownloadTableRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public byte[]	ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			if(	ClusterId.Length != Table<ITableEntry<int>, int>.Cluster.IdLength ||
				Offset < 0 ||
				Length < 0)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(sun.Lock)
			{
				RequireBase(sun);
				
				var m = Table switch
							  {
									Tables.Accounts	=> sun.Mcv.Accounts.Clusters.Find(i => i.Id.SequenceEqual(ClusterId))?.Main,
									Tables.Authors	=> sun.Mcv.Authors.Clusters.Find(i => i.Id.SequenceEqual(ClusterId))?.Main,
									Tables.Analyses	=> sun.Mcv.Releases.Clusters.Find(i => i.Id.SequenceEqual(ClusterId))?.Main,
									_ => throw new RequestException(RequestError.IncorrectRequest)
							  };

				if(m == null)
					throw new EntityException(EntityError.NotFound);
	
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
