namespace Uccs.Fair;

public class TopicRequest : FairPpc<TopicResponse>
{
	public new EntityId	Id { get; set; }

	public TopicRequest()
	{
	}

	public TopicRequest(EntityId id)
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

			var	e = Mcv.Topics.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new TopicResponse {Topic = e};
		}
	}
}

public class TopicResponse : PeerResponse
{
	public Topic	Topic {get; set;}
}
