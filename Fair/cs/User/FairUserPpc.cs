namespace Uccs.Fair;

public class FairUserByIdPpc : McvPpc<FairUserByIdPpr>, IBinarySerializable
{
	public AutoId	Id {get; set;}

	public FairUserByIdPpc()
	{
	}

	public FairUserByIdPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	u = Mcv.Users.Latest(Id) as FairUser
				??
				throw new EntityException(EntityError.NotFound);
			
		return new FairUserByIdPpr {User = u};
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

public class FairUserByIdPpr : Result, IBinarySerializable
{
	public FairUser User {get; set;}

	public void Read(Reader reader)
	{
		User = reader.Read<FairUser>();
	}

	public void Write(Writer writer)
	{	
		writer.Write(User);
	}
}

public class FairUserByNamePpc : McvPpc<FairUserByNamePpr>, IBinarySerializable
{
	public string	Name {get; set;}

	public FairUserByNamePpc()
	{
	}

	public FairUserByNamePpc(string identifier)
	{
		Name = identifier;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	u = Mcv.Users.Latest(Name) as FairUser
				??
				throw new EntityException(EntityError.NotFound);
			
		return new FairUserByNamePpr {User = u};
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

public class FairUserByNamePpr : Result, IBinarySerializable
{
	public FairUser User {get; set;}

	public void Read(Reader reader)
	{
		User = reader.Read<FairUser>();
	}

	public void Write(Writer writer)
	{
		writer.Write(User);
	}
}
