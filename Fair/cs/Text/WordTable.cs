namespace Uccs.Fair;

public class WordTable : Table<RawId, Word>
{
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
			
		a = Find(id);

		if(a == null)
		{
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