namespace Uccs.Fair;

public class CategoryTable : Table<AutoId, Category>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public CategoryTable(FairMcv rds) : base(rds, FairTable.Category.ToString())
	{
	}
	
	public override Category Create()
	{
		return new Category(Mcv);
	}
}

public class CategoryExecution : TableExecution<AutoId, Category, CategoryTable>
{
	public CategoryExecution(FairExecution execution) : base(execution.Mcv.Categories, execution)
	{
	}

	public Category Create(Store store)
	{
		Execution.IncrementMetaInt(FairMetaEntityType.CategoriesCount);

		var a = Table.Create();
		a.Id			= LastCreatedId = new AutoId(Execution.IncrementMetaInt(FairMetaEntityType.CategoriesIdCounter));
		a.Categories	= [];
		a.Publications	= [];

		return Affected[a.Id] = a;
	}
}
