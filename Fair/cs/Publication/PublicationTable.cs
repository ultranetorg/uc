namespace Uccs.Fair;

public class PublicationTable : Table<PublicationEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public PublicationTable(FairMcv rds) : base(rds)
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
