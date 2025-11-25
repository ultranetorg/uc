namespace Uccs.Net;

public class MembersPpc : McvPpc<MembersPpr>
{
	public override Return Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.VotersRound.Members.Count == 0)
				throw new EntityException(EntityError.NoMembers);

			return new MembersPpr {Members = Mcv.NextVotingRound.VotersRound.Members.ToArray()};
		}
	}
}

public class MembersPpr : Return
{
	public Generator[] Members { get; set; }
}
