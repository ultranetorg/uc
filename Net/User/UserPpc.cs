namespace Uccs.Net;

public class UserPpc : McvPpc<UserPpr>
{
	public AutoId		Id {get; set;}
	public string		Name {get; set;}

	public UserPpc()
	{
	}

	public UserPpc(string identifier)
	{
		Name = identifier;
	}

	public UserPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			User u;

			if(Id != null)
				u = Mcv.Users.Latest(Id);
			else if(Name != null)
				u = Mcv.Users.Latest(Name);
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(u == null)
				throw new EntityException(EntityError.NotFound);
			
			return new UserPpr {User = u};
		}
	}
}

public class UserPpr : Result
{
	public User User {get; set;}
}
