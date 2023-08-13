namespace Uccs.Net
{
	public class FileInfoRequest : RdcRequest
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		public string			File { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Resources == null) 
				throw new RdcNodeException(RdcNodeError.NotSeeder);
			
			if(!core.Resources.Exists(Resource, Hash, File)) 
				throw new RdcNodeException(RdcNodeError.NotFound);

			return new FileInfoResponse{Length = core.Resources.GetLength(Resource, Hash, File)};
		}
	}

	public class FileInfoResponse : RdcResponse
	{
		public long Length { get; set; }
	}
}
