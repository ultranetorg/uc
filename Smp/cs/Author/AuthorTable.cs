namespace Uccs.Smp;

public class AuthorTable : Table<AuthorEntry>
{
	public IEnumerable<SmpRound>	Tail => Mcv.Tail.Cast<SmpRound>();

	//public override bool			Equal(object a, object b) => a.Equals(b);
	//public override Span<byte>		KeyToCluster(object domain) => null;

	public AuthorTable(SmpMcv rds) : base(rds)
	{
	}
		
	public override AuthorEntry Create()
	{
		return new AuthorEntry(Mcv);
	}


	public AuthorEntry Find(EntityId id, int ridmax)
	{
		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
		//	throw new IntegrityException("maxrid works inside pool only");

		foreach(var r in Tail.Where(i => i.Id <= ridmax))
		{
			var a = r.AffectedAuthors.Values.FirstOrDefault(i => i.Id == id);
			
			if(a != null)
				return a;
		}

		return FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);
	}
}

