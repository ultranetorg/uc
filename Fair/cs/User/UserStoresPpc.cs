using System.Collections.Immutable;

namespace Uccs.Fair;

public class UserStoresPpc : McvPpc<UserStoresPpr>, IBinarySerializable
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
			
		return new UserStoresPpr {Stores = e.ModeratedStores.ToArray()};
	}

	public void Write(Writer writer)
	{
		writer.Write(User);
	}

	public void Read(Reader reader)
	{
		User = reader.Read<AutoId>();
	}
}

public class UserStoresPpr : Result, IBinarySerializable
{
	public AutoId[] Stores {get; set;}

	public void Read(Reader reader)
	{
		Stores = reader.ReadArray<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Stores);
	}
}
