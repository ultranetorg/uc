﻿namespace Uccs.Net
{
	public class AnalysisRequest : RdcRequest
	{
		public byte[] Release {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireSynchronizedBase(sun);

				var e = sun.Mcv.Analyses.Find(Release, sun.Mcv.LastConfirmedRound.Id); 

				if(e == null)
					throw new RdcEntityException(RdcEntityError.NotFound);

				return new AnalysisResponse {Analysis = e};
			}
		}
	}
	
	public class AnalysisResponse : RdcResponse
	{
		public Analysis Analysis {get; set;}
	}
}