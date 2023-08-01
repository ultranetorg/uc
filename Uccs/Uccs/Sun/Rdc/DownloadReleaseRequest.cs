namespace Uccs.Net
{
	public class DownloadReleaseRequest : RdcRequest
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		public string			File { get; set; }
		public long				Offset { get; set; }
		public long				Length { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Resources == null) 
				throw new RdcNodeException(RdcNodeError.NotSeeder);
			
			if(!core.Resources.Exists(Resource, Hash, File)) 
				throw new RdcNodeException(RdcNodeError.NotFound);

			return new DownloadReleaseResponse{Data = core.Resources.ReadFile(Resource, Hash, File, Offset, Length)};
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
