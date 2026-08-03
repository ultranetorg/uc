namespace Uccs.Fair;

public class CategoryPpc : FairPpc<CategoryPpr>
{
	public AutoId	Id { get; set; }

	public CategoryPpc()
	{
	}

	public CategoryPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	e = Mcv.Categories.Latest(Id);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new CategoryPpr {Category = e};
	}
	
	public override void Write(Writer writer)
	{
		writer.Write(Id);
	}

	public override void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}
}

public class CategoryPpr : Result
{
	public Category	Category {get; set;}

	public override void Read(Reader reader)
	{
		Category = reader.Read<Category>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Category);
	}
}
