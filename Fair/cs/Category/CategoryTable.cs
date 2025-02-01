namespace Uccs.Fair;

public class CategoryTable : Table<CategoryEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public CategoryTable(FairMcv rds) : base(rds)
	{
	}
	
	public override CategoryEntry Create()
	{
		return new CategoryEntry(Mcv);
	}

	public CategoryEntry Find(EntityId id, int ridmax)
	{
  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedCategories.TryGetValue(id, out var r))
    			return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		return e;
	}
 }
