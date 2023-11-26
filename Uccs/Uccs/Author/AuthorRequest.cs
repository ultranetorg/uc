namespace Uccs.Net
{
	public class AuthorRequest : RdcRequest
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			if(!Author.Valid(Name))	
				throw new RdcRequestException();

 			lock(sun.Lock)
			{	
				RequireBase(sun);

				var e = sun.Mcv.Authors.Find(Name, sun.Mcv.LastConfirmedRound.Id); 

				if(e == null)
					throw new RdcEntityException(RdcEntityError.NotFound);

				return new AuthorResponse {Author = e};
			}
		}
	}
	
	public class AuthorResponse : RdcResponse
	{
		public Author	Author {get; set;}
	}
}
