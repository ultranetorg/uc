﻿using System.Text;

namespace Uccs.Fair;

public class AuthorTable : Table<AuthorEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();

	//public override bool			Equal(object a, object b) => a.Equals(b);
	//public override Span<byte>		KeyToCluster(object domain) => null;

	public AuthorTable(FairMcv rds) : base(rds)
	{
	}
		
	protected override AuthorEntry Create(int cid)
	{
		return new AuthorEntry(Mcv) {Id = new EntityId {H = cid}};
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

		return FindBucket(id.H)?.Entries.Find(i => i.Id.E == id.E);
	}
}
