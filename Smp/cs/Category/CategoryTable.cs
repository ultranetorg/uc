namespace Uccs.Smp;

public class CategoryTable : Table<CategoryEntry>
{
	public IEnumerable<SmpRound>	Tail => Mcv.Tail.Cast<SmpRound>();
	public new SmpMcv				Mcv => base.Mcv as SmpMcv;

	public CategoryTable(SmpMcv rds) : base(rds)
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
