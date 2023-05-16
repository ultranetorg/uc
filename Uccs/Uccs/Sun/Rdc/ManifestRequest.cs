namespace Uccs.Net
{
	public class ManifestRequest : RdcRequest
	{
		public ReleaseAddress	Release { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Filebase == null) throw new RdcNodeException(RdcNodeError.NotSeed);

			return new ManifestResponse{Manifest = core.Filebase.FindRelease(Release)?.Manifest};
		}
	}
		
	public class ManifestResponse : RdcResponse
	{
		public Manifest Manifest { get; set; }
	}
}
