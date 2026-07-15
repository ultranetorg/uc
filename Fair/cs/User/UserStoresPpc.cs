namespace Uccs.Fair;

public class UserStoresPpc : McvPpc<UserStoresPpr>
{
	public AutoId		User {get; set;}

	public UserStoresPpc()
	{
	}

	public UserStoresPpc(AutoId name)
	{
		User = name;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	e = Mcv.Users.Latest(User) as FairUser;
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new UserStoresPpr {Stores = e.ModeratedStores};
	}
}

public class UserStoresPpr : Result
{
	public AutoId[] Stores {get; set;}
}
