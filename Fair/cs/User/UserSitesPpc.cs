namespace Uccs.Fair;

public class UserSitesPpc : McvPpc<UserSitesPpr>
{
	public string		Name {get; set;}

	public UserSitesPpc()
	{
	}

	public UserSitesPpc(string name)
	{
		Name = name;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var	e = Mcv.Users.Latest(Name) as FairUser;
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new UserSitesPpr {Sites = e.ModeratedSites};
		}
	}
}

public class UserSitesPpr : Result
{
	public AutoId[] Sites {get; set;}
}
