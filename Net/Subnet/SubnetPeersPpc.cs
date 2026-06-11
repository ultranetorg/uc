namespace Uccs.Net;

public class SubnetPeersPpc : McvPpc<SubnetPeersPpr>
{
	public string Name { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.Senders.Count() == 0)
				throw new EntityException(EntityError.NoMembers);

			var f = Mcv.Friends.Latest(Name);

			if(f == null)
				throw new EntityException(EntityError.NotFound);

			return new SubnetPeersPpr {Endpoints = f.Peers};
		}
	}
}

public class SubnetPeersPpr : Result
{
	public Endpoint[] Endpoints { get; set; }
}
