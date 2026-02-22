namespace Uccs.Net;

public class MembersPpc : McvPpc<MembersPpr>
{
	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.Voters.Count() == 0)
				throw new EntityException(EntityError.NoMembers);

			return new MembersPpr {Members = Mcv.NextVotingRound.Voters.ToArray()};
		}
	}
}

public class MembersPpr : Result
{
	public Generator[] Members { get; set; }
}
