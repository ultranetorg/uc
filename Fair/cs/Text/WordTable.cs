namespace Uccs.Fair;

public class WordTable : Table<RawId, Word>
{
	public override string			Name => FairTable._Word.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	public override bool			IsIndex => true;

	public WordTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Word Create()
	{
		return new Word(Mcv);
	}

	public IEnumerable<AutoId> Search(EntityTextField field, string prefix, int count)
	{
		var id = Word.GetId(prefix);

		int n = 0;

		var found = new HashSet<AutoId>();

		foreach(var i in Tail)
		{
			foreach(var r in i.Words.Affected.Where(i => i.Key.Bytes.Take(id.Bytes.Length).SequenceEqual(id.Bytes)).Where(i => i.Value.Reference.Field == field).Select(i => i.Value.Reference))
			{
				if(!found.Contains(r.Entity))
				{
					found.Add(r.Entity);
					yield return r.Entity;

					n++;

					if(n > count)
						yield break;
				}
			}
		}
						
		var b = FindBucket(id.B);

		if(b != null)
		{
			foreach(var r in b.Entries.Where(i => i.Reference.Field == field && i.Id.Bytes.Take(id.Bytes.Length).SequenceEqual(id.Bytes)).Select(i => i.Reference))
			{
				if(!found.Contains(r.Entity))
				{
					found.Add(r.Entity);
					yield return r.Entity;
	
					n++;
		
					if(n > count)
						yield break;
				}
			}
		}

		if(id.Bytes.Length <= 2)
		{
			var c = FindCluster((short)(id.B >> 2 & 0x3ff));
			
			if(c != null)
				foreach(var i in c.Buckets.Where(b => (b.Id >> 8 & 0b11) == (id.B & 0b11)))
				{
					foreach(var r in i.Entries.Where(i => i.Reference.Field == field).Select(i => i.Reference))
					{
						if(!found.Contains(r.Entity))
						{
							found.Add(r.Entity);
							yield return r.Entity;
	
							n++;
	
							if(n > count)
								yield break;
						}
					}
				}
		}

		/// MAY BE TOO SLOW
		if(id.Bytes.Length == 1)
		{
			foreach(var c in Clusters.Where(c => (c.Id >> 6) == (id.B & 0xf)).SelectMany(j => j.Buckets))
			{
				foreach(var r in c.Entries.Where(i => i.Reference.Field == field && i.Id.Bytes[0] == id.Bytes[0]).Select(i => i.Reference))
				{
					if(!found.Contains(r.Entity))
					{
						found.Add(r.Entity);
						yield return r.Entity;
	
						n++;
	
						if(n > count)
							yield break;
					}
				}
			}
		}
	}
 }

public class WordExecution : TableExecution<RawId, Word>
{
	public WordExecution(FairExecution execution) : base(execution.Mcv.Words, execution)
	{
	}

	public override Word Affect(RawId id)
	{
		if(Affected.TryGetValue(id, out var a))
			return a;
			
		a = Table.Find(id, Execution.Round.Id);

		if(a == null)
		{
			Execution.IncrementCount((int)FairMetaEntityType.WordsCount);

			a = Table.Create();
			a.Id = id;
		
			return Affected[id] = a;
		} 
		else
		{
			return Affected[id] = a.Clone() as Word;
		}
	}

	public void Register(string word, EntityTextField field, AutoId entity)
	{
		var id = Word.GetId(word);
		var w = Affect(id);
	
		w.Reference = new EntityFieldAddress {Entity = entity, Field = field};
	}

	public void Unregister(string word)
	{
		var id = Word.GetId(word);
		var w = Affect(id);

		w.Deleted = true;
	}
}