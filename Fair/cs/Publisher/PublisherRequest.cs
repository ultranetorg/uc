namespace Uccs.Fair
{
	public class PublisherRequest : FairPpc<PublisherResponse>
	{
		public new EntityId	Id { get; set; }

		public PublisherRequest()
		{
		}

		public PublisherRequest(EntityId id)
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

				var	e = Mcv.Publishers.Find(Id, Mcv.LastConfirmedRound.Id);
				
				if(e == null)
					throw new EntityException(EntityError.NotFound);
				
				return new PublisherResponse {Publisher = e};
			}
		}
	}
	
	public class PublisherResponse : PeerResponse
	{
		public Publisher	Publisher {get; set;}
	}
}
