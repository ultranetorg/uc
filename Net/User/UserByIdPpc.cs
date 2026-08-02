namespace Uccs.Net;

public class UserByIdPpc : McvPpc<UserByIdPpr>, IBinarySerializable
{
	public AutoId	Id {get; set;}

	public UserByIdPpc()
	{
	}

	public UserByIdPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	u = Mcv.Users.Latest(Id)
				??
				throw new EntityException(EntityError.NotFound);
			
		return new UserByIdPpr {User = u};
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
	}

	public void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}
}

public class UserByIdPpr : Result, IBinarySerializable
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
