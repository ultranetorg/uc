namespace Uccs.Fair;

public class UserAuthorsPpc : McvPpc<UserAuthorsPpr>
{
	public string		Name {get; set;}

	public UserAuthorsPpc()
	{
	}

	public UserAuthorsPpc(string name)
	{
		Name = name;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var	e = Mcv.Users.Find(Name, Mcv.LastConfirmedRound.Id) as FairUser;
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new UserAuthorsPpr {Authors = e.Authors};
		}
	}
}

public class UserAuthorsPpr : Result
{
	public AutoId[] Authors {get; set;}
}
