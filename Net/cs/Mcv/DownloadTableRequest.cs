namespace Uccs.Net
{
	public class DownloadTableRequest : McvCall<DownloadTableResponse>
	{
		public int		Table { get; set; }
		public byte[]	Hash { get; set; }
		public byte[]	ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override PeerResponse Execute()
		{
			if(	ClusterId.Length != TableBase.ClusterBase.IdLength ||
				Offset < 0 ||
				Length < 0)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Mcv.Lock)
			{
				RequireBase();

				var c = Mcv.Tables[Table].Clusters.FirstOrDefault(i => i.Id.SequenceEqual(ClusterId));

				if(c == null)
					throw new EntityException(EntityError.NotFound);
	
				if(!c.Hash.SequenceEqual(Hash))
					throw new EntityException(EntityError.HashMismatach);

				var s = new MemoryStream(c.Main);
				var r = new BinaryReader(s);
	
				s.Position = Offset;
	
				return new DownloadTableResponse{Data = r.ReadBytes((int)Length)};
			}
		}
	}
		
	public class DownloadTableResponse : PeerResponse
	{
		public byte[] Data { get; set; }
	}
}
