namespace Uccs.Net
{
	public class AnalysisRequest : RdcRequest
	{
		public byte[] Release {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			if(Release.Length != Cryptography.HashSize)
				throw new RequestException();

 			lock(sun.Lock)
			{	
				RequireBase(sun);

				var e = sun.Mcv.Analyses.Find(Release, sun.Mcv.LastConfirmedRound.Id); 

				if(e == null)
					throw new EntityException(EntityError.NotFound);

				return new AnalysisResponse {Analysis = e};
			}
		}
	}
	
	public class AnalysisResponse : RdcResponse
	{
		public Analysis Analysis {get; set;}
	}
}
