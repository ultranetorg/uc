namespace Uccs.Net;

public class UserByNamePpc : McvPpc<UserByNamePpr>, IBinarySerializable
{
	public string	Name {get; set;}

	public UserByNamePpc()
	{
	}

	public UserByNamePpc(string identifier)
	{
		Name = identifier;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	u = Mcv.Users.Latest(Name)
				??
				throw new EntityException(EntityError.NotFound);
			
		return new UserByNamePpr {User = u};
	}

	public void Write(Writer writer)
	{
		writer.WriteASCII(Name);
	}

	public void Read(Reader reader)
	{
		Name = reader.ReadASCII();
	}
}

public class UserByNamePpr : Result, IBinarySerializable
{
	public User User {get; set;}

	public void Read(Reader reader)
	{
		User = reader.Read<User>();
	}

	public void Write(Writer writer)
	{
		writer.Write(User);
	}
}
