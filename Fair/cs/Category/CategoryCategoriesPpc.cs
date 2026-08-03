
namespace Uccs.Fair;

public class CategoryCategoriesPpc : FairPpc<CategoryCategoriesPpr>
{
	public AutoId		Category {get; set;}

	public CategoryCategoriesPpc()
	{
	}

	public CategoryCategoriesPpc(AutoId id)
	{
		Category = id;
	}

	public override Result Execute()
	{
		RequireGraph();

		var e = Mcv.Categories.Latest(Category);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new CategoryCategoriesPpr {Categories = e.Categories};
	}

	public override void Read(Reader reader)
	{
		Category = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Category);
	}
}

public class CategoryCategoriesPpr : Result
{
	public AutoId[] Categories {get; set;}

	public override void Read(Reader reader)
	{
		Categories = reader.ReadArray<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Categories);
	}
}
