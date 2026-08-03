namespace Uccs.Fair;

public class AuthorPpc : FairPpc<AuthorPpr>
{
	public AutoId	Id { get; set; }

	public AuthorPpc()
	{
	}

	public AuthorPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

		RequireGraph();

		var	e = Mcv.Authors.Latest(Id);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new AuthorPpr {Author = e};
	}

	public override void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
	}
}

public class AuthorPpr : Result
{
	public Author	Author {get; set;}

	public override void Read(Reader reader)
	{
		Author = reader.Read<Author>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Author);
	}
}

public class AuthorByNamePpc : FairPpc<AuthorByNamePpr>
{
	public string	Name { get; set; }

	public AuthorByNamePpc()
	{
	}

	public AuthorByNamePpc(string id)
	{
		Name = id;
	}

	public override Result Execute()
	{
		if(Name == null)
			throw new RequestException(RequestError.IncorrectRequest);

		RequireGraph();

		var	w = Mcv.Words.Latest(Word.GetId(Name));
		
		if(w == null || w.Reference.Field != EntityTextField.AuthorName)
			throw new EntityException(EntityError.NotFound);

		var	a = Mcv.Authors.Latest(w.Reference.Entity);
			
		if(a == null)
			throw new IntegrityException();
			
		return new AuthorPpr {Author = a};
	}

	public override void Read(Reader reader)
	{
		Name = reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
	}
}

public class AuthorByNamePpr : Result
{
	public Author	Author {get; set;}

	public override void Read(Reader reader)
	{
		Author = reader.Read<Author>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Author);
	}
}
