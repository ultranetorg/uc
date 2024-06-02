using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class DownloadTableRequest : RdcCall<DownloadTableResponse>
	{
		public int		Table { get; set; }
		public byte[]	ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override RdcResponse Execute()
		{
			if(	ClusterId.Length != TableBase.ClusterBase.IdLength ||
				Offset < 0 ||
				Length < 0)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Mcv.Lock)
			{
				RequireBase();

				var m = Mcv.Tables[Table].Clusters.FirstOrDefault(i => i.Id.SequenceEqual(ClusterId))?.Main;

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
