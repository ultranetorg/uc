namespace Uccs.Net
{
	public class DomainRequest : RdcCall<DomainResponse>
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			if(!Domain.Valid(Name))	
				throw new RequestException(RequestError.IncorrectRequest);

 			lock(sun.Lock)
			{	
				RequireBase(sun);

				var e = sun.Mcv.Domains.Find(Name, sun.Mcv.LastConfirmedRound.Id); 

				if(e == null)
					throw new EntityException(EntityError.NotFound);

				return new DomainResponse {Domain = e, EntityId = e.Id};
			}
		}
	}
	
	public class DomainResponse : RdcResponse
	{
		public EntityId	EntityId {get; set;} = new([0,0], 0);
		public Domain	Domain {get; set;}
	}
}
