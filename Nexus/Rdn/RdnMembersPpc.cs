namespace Uccs.Rdn;

public class RdnMembersPpc : McvPpc<RdnMembersPpr>
{
	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();
		
			if(Mcv.NextVotingRound.Voters.Count() == 0)
				throw new EntityException(EntityError.NoMembers);

			return new RdnMembersPpr {Members = Mcv.NextVotingRound.Voters.Cast<RdnGenerator>().ToArray()};
		}
	}
}

public class RdnMembersPpr : Result
{
	public RdnGenerator[] Members { get; set; }
}
