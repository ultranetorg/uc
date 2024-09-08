namespace Uccs.Rdn
{
	public class RdnMembersRequest : McvCall<RdnMembersResponse>
	{
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
			
				if(Mcv.NextVoteMembers.Count == 0)
					throw new EntityException(EntityError.NoMembers);

				return new RdnMembersResponse {Members = Mcv.NextVoteMembers.Cast<RdnMember>().ToArray()};
			}
		}
	}

	public class RdnMembersResponse : PeerResponse
	{
		public RdnMember[] Members { get; set; }
	}
}
