namespace Uccs.Net
{
	public class FileInfoRequest : RdcRequest
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		public string			File { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			if(sun.ResourceHub == null) 
				throw new RdcNodeException(RdcNodeError.NotSeed);
			
			if(!sun.ResourceHub.Exists(Resource, Hash, File)) 
				throw new RdcNodeException(RdcNodeError.NotFound);

			return new FileInfoResponse{Length = sun.ResourceHub.GetLength(Resource, Hash, File)};
		}
	}

	public class FileInfoResponse : RdcResponse
	{
		public long Length { get; set; }
	}
}
