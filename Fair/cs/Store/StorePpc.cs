namespace Uccs.Fair;

public class StorePpc : FairPpc<StorePpr>
{
	public AutoId	Id { get; set; }

	public StorePpc()
	{
	}

	public StorePpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

		RequireGraph();

		var	e = Mcv.Stores.Latest(Id);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new StorePpr {Store = e};
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

public class StorePpr : Result
{
	public Store	Store {get; set;}

	public override void Read(Reader reader)
	{
		Store = reader.Read<Store>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Store);
	}
}

public class StoreByNamePpc : FairPpc<StoreByNamePpr>
{
	public string	Name { get; set; }

	public StoreByNamePpc()
	{
	}

	public StoreByNamePpc(string id)
	{
		Name = id;
	}

	public override Result Execute()
	{
		if(Name == null)
			throw new RequestException(RequestError.IncorrectRequest);

		RequireGraph();

		var	w = Mcv.Names.Latest(NameTable.GetId(Name));
		
		if(w == null || w.Entity.Field != EntityTextField.StoreName)
			throw new EntityException(EntityError.NotFound);

		var	a = Mcv.Stores.Latest(w.Entity.Id);
			
		if(a == null)
			throw new IntegrityException();
			
		return new StorePpr {Store = a};
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

public class StoreByNamePpr : Result
{
	public Store	Store {get; set;}

	public override void Read(Reader reader)
	{
		Store = reader.Read<Store>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Store);
	}
}
