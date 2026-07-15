namespace Uccs.Fair;

public class UserAuthorsPpc : McvPpc<UserAuthorsPpr>
{
	public AutoId		User {get; set;}

	public UserAuthorsPpc()
	{
	}

	public UserAuthorsPpc(AutoId id)
	{
		User = id;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	e = Mcv.Users.Latest(User) as FairUser;
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new UserAuthorsPpr {Authors = e.Authors};
	}
}

public class UserAuthorsPpr : Result
{
	public AutoId[] Authors {get; set;}
}
