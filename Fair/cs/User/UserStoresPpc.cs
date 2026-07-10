namespace Uccs.Fair;

public class UserStoresPpc : McvPpc<UserStoresPpr>
{
	public string		Name {get; set;}

	public UserStoresPpc()
	{
	}

	public UserStoresPpc(string name)
	{
		Name = name;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	e = Mcv.Users.Latest(Name) as FairUser;
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new UserStoresPpr {Stores = e.ModeratedStores};
	}
}

public class UserStoresPpr : Result
{
	public AutoId[] Stores {get; set;}
}
