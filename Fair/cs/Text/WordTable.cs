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
			foreach(var j in i.Words.Affected.Where(i => i.Key.Bytes.Take(id.Bytes.Length).SequenceEqual(id.Bytes)))
			{
				var r = j.Value.References.FirstOrDefault(i => i.Field == field);
					
				if(r != null && !found.Contains(r.Entity))
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
			foreach(var i in b.Entries.Where(i => i.References.Any(j => j.Field == field) && i.Id.Bytes.Take(id.Bytes.Length).SequenceEqual(id.Bytes)))
			{
				var r = i.References.First(j => j.Field == field);

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
			a.References = [];
		
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
		var w = Find(id)?.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);;
		
		if(w == null)
		{
			var t = Affect(id);
	
			t.References = [..t.References, new EntityFieldAddress {Entity = entity, Field = field}];
		}
	}

	public void Unregister(string word, EntityTextField field, AutoId entity)
	{
		var id = Word.GetId(word);
		var w = Find(id)?.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);;
		
		if(w == null)
		{
			var t = Affect(id);
	
			t.References = t.References.Remove(w);
		}
	}
}