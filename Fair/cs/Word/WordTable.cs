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
		if(prefix.Length < 3)
			yield break;

		var pre = Word.GetId(prefix);

		int n = 0;

		var found = new HashSet<AutoId>();

		foreach(var i in Tail)
		{
			foreach(var r in i.Words.Affected.Where(i => i.Key.Bytes.Take(pre.Bytes.Length).SequenceEqual(pre.Bytes)).Where(i => i.Value.Reference.Field == field).Select(i => i.Value.Reference))
			{
				if(found.Add(r.Entity))
				{
					yield return r.Entity;

					n++;

					if(n > count)
						yield break;
				}
			}
		}
						
		var b = FindBucket(pre.B);

		if(b != null)
		{
			foreach(var r in b.Entries.Where(i => i.Reference.Field == field && i.Id.Bytes.Take(pre.Bytes.Length).SequenceEqual(pre.Bytes)).Select(i => i.Reference))
			{
				if(found.Add(r.Entity))
				{
					yield return r.Entity;
	
					n++;
		
					if(n > count)
						yield break;
				}
			}
		}

//		if(pre.Bytes.Length == 2)
//		{
//			var c = FindCluster(ClusterFromBucket(pre.B));
//			
//			if(c != null)
//				foreach(var i in c.Buckets.Where(b => (b.Id >> 8 & 0b11) == (id.B & 0b11)))
//				{
//					foreach(var r in i.Entries.Where(i => i.Reference.Field == field).Select(i => i.Reference))
//					{
//						if(!found.Contains(r.Entity))
//						{
//							found.Add(r.Entity);
//							yield return r.Entity;
//	
//							n++;
//	
//							if(n > count)
//								yield break;
//						}
//					}
//				}
//		}
//
//		/// MAY BE TOO SLOW
//		if(pre.Bytes.Length == 1)
//		{
//			foreach(var c in Clusters.Where(c => (c.Id >> 6) == (pre.B & 0xf)).SelectMany(j => j.Buckets))
//			{
//				foreach(var r in c.Entries.Where(i => i.Reference.Field == field && i.Id.Bytes[0] == pre.Bytes[0]).Select(i => i.Reference))
//				{
//					if(!found.Contains(r.Entity))
//					{
//						found.Add(r.Entity);
//						yield return r.Entity;
//	
//						n++;
//	
//						if(n > count)
//							yield break;
//					}
//				}
//			}
//		}
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