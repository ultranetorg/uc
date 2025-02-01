namespace Uccs.Smp;

public class PublicationTable : Table<PublicationEntry>
{
	public IEnumerable<SmpRound>	Tail => Mcv.Tail.Cast<SmpRound>();
	public new SmpMcv				Mcv => base.Mcv as SmpMcv;

	public PublicationTable(SmpMcv rds) : base(rds)
	{
	}
	
	public override PublicationEntry Create()
	{
		return new PublicationEntry(Mcv);
	}

	public PublicationEntry Find(EntityId id, int ridmax)
	{
  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedPublications.TryGetValue(id, out var r))
    			return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		return e;
	}
 }
