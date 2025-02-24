namespace Uccs.Fair;

public class DisputeTable : Table<DisputeEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public DisputeTable(FairMcv rds) : base(rds)
	{
	}
	
	public override DisputeEntry Create()
	{
		return new DisputeEntry(Mcv);
	}

	public DisputeEntry Find(EntityId id, int ridmax)
	{
  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedDisputes.TryGetValue(id, out var r))
    			return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		return e;
	}
 }
