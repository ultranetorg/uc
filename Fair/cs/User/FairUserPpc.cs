namespace Uccs.Fair;

public class FairUserPpc : McvPpc<FairUserPpr>
{
	public AutoId		Id {get; set;}
	public string		Name {get; set;}

	public FairUserPpc()
	{
	}

	public FairUserPpc(string identifier)
	{
		Name = identifier;
	}

	public FairUserPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			FairUser u;

			if(Id != null)
				u = Mcv.Users.Latest(Id) as FairUser;
			else if(Name != null)
				u = Mcv.Users.Latest(Name) as FairUser;
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(u == null)
				throw new EntityException(EntityError.NotFound);
			
			return new FairUserPpr {User = u};
		}
	}
}

public class FairUserPpr : Result
{
	public FairUser User {get; set;}
}
