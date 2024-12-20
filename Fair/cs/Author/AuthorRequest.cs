namespace Uccs.Fair
{
	public class AuthorRequest : FairPpc<PublisherResponse>
	{
		public new EntityId	Id { get; set; }

		public AuthorRequest()
		{
		}

		public AuthorRequest(EntityId id)
		{
			Id = id;
		}

		public override PeerResponse Execute()
		{
			if(Id == null)
				throw new RequestException(RequestError.IncorrectRequest);

 			lock(Mcv.Lock)
			{	
				RequireBase();

				var	e = Mcv.Authors.Find(Id, Mcv.LastConfirmedRound.Id);
				
				if(e == null)
					throw new EntityException(EntityError.NotFound);
				
				return new PublisherResponse {Author = e};
			}
		}
	}
	
	public class PublisherResponse : PeerResponse
	{
		public Author	Author {get; set;}
	}
}
