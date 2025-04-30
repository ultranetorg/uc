namespace Uccs.Fair;

public class CategoryTable : Table<AutoId, Category>
{
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
		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = new AutoId(site.Id.B, e);
		a.Categories = [];
		a.Publications = [];

		return Affected[a.Id] = a;
	}
		
	public Category Affect(AutoId id)
	{
		if(Affected.TryGetValue(id, out var a))
			return a;
			
		var e = Find(id);

		return Affected[id] = e.Clone();
	}
}
