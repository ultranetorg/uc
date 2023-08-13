namespace Uccs.Net
{
	public class AuthorRequest : RdcRequest
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				if(Name.Length == 0)									throw new RdcEntityException(RdcEntityError.InvalidRequest);
				if(!sun.Settings.Roles.HasFlag(Role.Base))				throw new RdcNodeException(RdcNodeError.NotBase);
				if(sun.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				return new AuthorResponse{Entry = sun.Mcv.Authors.Find(Name, sun.Mcv.LastConfirmedRound.Id)};
			}
		}
	}
	
	public class AuthorResponse : RdcResponse
	{
		public Author Entry {get; set;}
	}
}
