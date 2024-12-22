namespace Uccs.Net;

public class MembersRequest : McvPpc<MembersResponse>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireBase();
		
			if(Mcv.NextVoteRound.VotersRound.Members.Count == 0)
				throw new EntityException(EntityError.NoMembers);

			return new MembersResponse {Members = Mcv.NextVoteRound.VotersRound.Members.ToArray()};
		}
	}
}

public class MembersResponse : PeerResponse
{
	public Generator[] Members { get; set; }
}
