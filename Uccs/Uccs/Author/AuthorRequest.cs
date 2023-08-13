namespace Uccs.Net
{
	public class AuthorRequest : RdcRequest
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(Name.Length == 0)										throw new RdcEntityException(RdcEntityError.InvalidRequest);
				if(!core.Settings.Roles.HasFlag(Role.Base))					throw new RdcNodeException(RdcNodeError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				return new AuthorResponse{Entry = core.Chainbase.Authors.Find(Name, core.Chainbase.LastConfirmedRound.Id)};
			}
		}
	}
	
	public class AuthorResponse : RdcResponse
	{
		public Author Entry {get; set;}
	}
}
