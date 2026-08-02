using System.Collections.Immutable;

namespace Uccs.Fair;

public class UserAuthorsPpc : McvPpc<UserAuthorsPpr>, IBinarySerializable
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
			
		return new UserAuthorsPpr {Authors = e.Authors.ToArray()};
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

public class UserAuthorsPpr : Result, IBinarySerializable
{
	public AutoId[] Authors {get; set;}

	public void Read(Reader reader)
	{
		Authors = reader.ReadArray<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Authors);
	}
}
