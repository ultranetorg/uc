namespace Uccs.Net
{
	public class DownloadReleaseRequest : RdcRequest
	{
		public ResourceAddress		Release { get; set; }
		public string				File { get; set; }
		public long					Offset { get; set; }
		public long					Length { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Filebase == null) throw new RdcNodeException(RdcNodeError.NotSeed);

			return new DownloadReleaseResponse{Data = core.Filebase.ReadFile(Release, File, Offset, Length)};
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
}
