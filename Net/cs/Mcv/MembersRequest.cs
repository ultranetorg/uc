namespace Uccs.Net
{
	public class MembersRequest : McvCall<MembersResponse>
	{
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
			
				if(Mcv.NextVoteMembers.Count == 0)
					throw new EntityException(EntityError.NoMembers);

				return new MembersResponse {Members = Mcv.NextVoteMembers.ToArray()};
			}
		}
	}

	public class MembersResponse : PeerResponse
	{
		public Generator[] Members { get; set; }
	}
}
