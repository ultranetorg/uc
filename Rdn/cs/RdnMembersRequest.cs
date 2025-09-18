namespace Uccs.Rdn;

public class RdnMembersRequest : McvPpc<RdnMembersResponse>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.VotersRound.Members.Count == 0)
				throw new EntityException(EntityError.NoMembers);

			return new RdnMembersResponse {Members = Mcv.NextVotingRound.VotersRound.Members.Cast<RdnGenerator>().ToArray()};
		}
	}
}

public class RdnMembersResponse : PeerResponse
{
	public RdnGenerator[] Members { get; set; }
}
