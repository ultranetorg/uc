namespace Uccs.Net
{
	public class FileInfoRequest : RdcRequest
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		public string			File { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
			if(sun.Resources == null) 
				throw new RdcNodeException(RdcNodeError.NotSeeder);
			
			if(!sun.Resources.Exists(Resource, Hash, File)) 
				throw new RdcNodeException(RdcNodeError.NotFound);

			return new FileInfoResponse{Length = sun.Resources.GetLength(Resource, Hash, File)};
		}
	}

	public class FileInfoResponse : RdcResponse
	{
		public long Length { get; set; }
	}
}
