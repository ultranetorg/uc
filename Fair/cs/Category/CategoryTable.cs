﻿namespace Uccs.Fair;

public class CategoryTable : Table<AutoId, Category>
{
	public override string			Name => FairTable.Category.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public CategoryTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Category Create()
	{
		return new Category(Mcv);
	}
}

public class CategoryExecution : TableExecution<AutoId, Category>
{
	public CategoryExecution(FairExecution execution) : base(execution.Mcv.Categories, execution)
	{
	}

	public Category Create(Site site)
	{
		Execution.IncrementCount((int)FairMetaEntityType.CategoriesCount);

		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(site.Id.B, e);
		a.Categories = [];
		a.Publications = [];

		return Affected[a.Id] = a;
	}
}
