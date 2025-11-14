namespace Uccs.Rdn;

public class RdnMembersPpc : McvPpc<RdnMembersPpr>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.VotersRound.Members.Count == 0)
				throw new EntityException(EntityError.NoMembers);

			return new RdnMembersPpr {Members = Mcv.NextVotingRound.VotersRound.Members.Cast<RdnGenerator>().ToArray()};
		}
	}
}

public class RdnMembersPpr : PeerResponse
{
	public RdnGenerator[] Members { get; set; }
}
