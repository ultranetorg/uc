namespace Uccs.Net
{
	public class ReleaseRequest : RdcRequest
	{
		public ReleaseAddress		Release { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireBase(sun);
 			
				var r = sun.Mcv.Releases.Find(Release, sun.Mcv.LastConfirmedRound.Id);
			
				if(r == null)
					throw new EntityException(EntityError.NotFound);
				
				return new ReleaseResponse {Release = r};
			}
		}
	}
		
	public class ReleaseResponse : RdcResponse
	{
		public Release Release { get; set; }
	}
}
