namespace Uccs.Net
{
	public class AuthorRequest : RdcRequest
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				if(!Author.Valid(Name))									throw new RdcEntityException(RdcEntityError.InvalidRequest);
				if(!sun.Roles.HasFlag(Role.Base))						throw new RdcNodeException(RdcNodeError.NotBase);
				if(sun.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var e = sun.Mcv.Authors.Find(Name, sun.Mcv.LastConfirmedRound.Id); 

				if(e == null)
					throw new RdcEntityException(RdcEntityError.NotFound);

				return new AuthorResponse {Author = e};
			}
		}
	}
	
	public class AuthorResponse : RdcResponse
	{
		public Author Author {get; set;}
	}
}
