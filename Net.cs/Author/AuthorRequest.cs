namespace Uccs.Net
{
	public class AuthorRequest : RdcRequest
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			if(!Author.Valid(Name))	
				throw new RequestException(RequestError.IncorrectRequest);

 			lock(sun.Lock)
			{	
				RequireBase(sun);

				var e = sun.Mcv.Authors.Find(Name, sun.Mcv.LastConfirmedRound.Id); 

				if(e == null)
					throw new EntityException(EntityError.NotFound);

				return new AuthorResponse {Author = e, EntityId = e.Id};
			}
		}
	}
	
	public class AuthorResponse : RdcResponse
	{
		public EntityId	EntityId {get; set;} = new([0,0], 0);
		public Author	Author {get; set;}
	}
}
