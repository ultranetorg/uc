namespace Uccs.Rdn
{
	public class RdnMembersRequest : McvCall<RdnMembersResponse>
	{
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
			
				if(Mcv.NextVoteRound.VotersRound.Members.Count == 0)
					throw new EntityException(EntityError.NoMembers);

				return new RdnMembersResponse {Members = Mcv.NextVoteRound.VotersRound.Members.Cast<RdnGenerator>().ToArray()};
			}
		}
	}

	public class RdnMembersResponse : PeerResponse
	{
		public RdnGenerator[] Members { get; set; }
	}
}
