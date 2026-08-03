namespace Uccs.Net;

public class UserByNamePpc : McvPpc<UserByNamePpr>
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

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
	}

	public override void Read(Reader reader)
	{
		Name = reader.ReadASCII();
	}
}

public class UserByNamePpr : Result
{
	public User User {get; set;}

	public override void Read(Reader reader)
	{
		User = reader.Read<User>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(User);
	}
}
