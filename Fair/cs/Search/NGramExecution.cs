using System.Collections;
using System.Text;
using Roaring.Net.CRoaring;

namespace Uccs.Fair;

public abstract class NgramExecution<ID> : NgramTableState<ID> where ID : EntityId, new()
{
	public FairExecution				Execution;
	public NgramExecution<ID>			Parent;
	public int							Q => Table.Q;

	protected NgramExecution(FairExecution execution, NgramTable<ID> table) : base(table)
	{
		Execution = execution;
	}

	public Ngram<ID> Find(ID id)
 	{
		if(Affected.TryGetValue(id, out var e))
			return e.Deleted ? null : e;

		if(Parent != null)
			return Parent.Find(id);

 		if(Execution.Round.FindState<NgramTableState<ID>>(Table).Affected.TryGetValue(id, out e))
			return e.Deleted ? null : e;
 		
		return Table.Find(id);
 	}

	public virtual Ngram<ID> Affect(ID id)
	{
 		if(Affected.TryGetValue(id, out var a))
 			return a;
 		
		if(Parent != null)
			a = Parent.Find(id);
		else if(!Execution.Round.FindState<NgramTableState<ID>>(Table).Affected.TryGetValue(id, out a))
			a = Table.Find(id);
 
		a = a.Clone() as Ngram<ID>;

 		return Affected[id] = a;
	}

  	public void Index(string text, object more, AutoId to)
  	{
		if(string.IsNullOrWhiteSpace(text))
			throw new IntegrityException();

        ReadOnlySpan<char> span = text.AsSpan();

		if(span.Length < Q)
		{
			Span<char> padded = stackalloc char[Q];
			span.CopyTo(padded);
			padded[span.Length..].Fill('_');
    
			// PackNGram сам переведет все символы в нижний регистр
			Index(padded, more, to);
			
			return;
		}

        for(int i = 0; i <= span.Length - Q; i++)
        {
            Index(span.Slice(i, Q), more, to);
        }
  	}
	
  	public void Deindex(string text, object more, AutoId id)
  	{
		if(string.IsNullOrWhiteSpace(text))
			throw new IntegrityException();

        ReadOnlySpan<char> span = text.AsSpan();

		if(span.Length < Q)
		{
			Span<char> padded = stackalloc char[Q];
			span.CopyTo(padded);
			padded[span.Length..].Fill('_');
    
			Deindex(padded, more, id);
			
			return;
		}

        for(int i = 0; i <= span.Length - Q; i++)
        {
            Deindex(span.Slice(i, Q), more, id);
        }
  	}

	Ngram<ID> Index(ReadOnlySpan<char> ngramSpan, object more, AutoId entity)
	{
		var id = Table.CreateId(Table.PackNGram(ngramSpan), more);

 		var e =	Find(id);
 
  		if(e == null)
  		{
 			e = Table.Create();
 			e.Id = id;
  			e.Entities  = new();
 		
 			Affected[id] = e;
  		}
  		else
		{ 
 			e = Affect(id);
			e.Entities = e.Entities.Clone();
		}

		e.Entities.Add(entity.ToULong());

		return e;
	}

	void Deindex(ReadOnlySpan<char> ngramSpan, object more, AutoId entity)
	{
		var id = Table.CreateId(Table.PackNGram(ngramSpan), more);

 		var e =	Find(id);
 
  		if(e == null)
			throw new IntegrityException();

		e = Affect(id);

		e.Entities = e.Entities.Clone();

		e.Entities.Remove(entity.ToULong());
	}

	public bool IsIndexed(ReadOnlySpan<char> text, object more, AutoId id)
	{
		if(text.IsEmpty)
			return false;

		ulong k;

		if(text.Length < Q)
		{
			Span<char> padded = stackalloc char[Q];
			text.CopyTo(padded);
			padded[text.Length..].Fill('_');
			k = Table.PackNGram(padded);
		}
		else
		{
			k = Table.PackNGram(text[..Q]);
		}

		var e = Find(Table.CreateId(k, more));
		
		return e?.Entities.Contains(id.ToULong()) ?? false; 
	}
	//
	//	public bool TryFind(string query, object more, out uint docId)
	//	{
	//		docId = default;
	//
	//		ulong[] candidates = Table.Search(query, more, Find, 0, 1, minMatchPercent: 100);
	//
	//		if(candidates.Length == 0)
	//			return false;
	//
	//		ReadOnlySpan<char> querySpan = query.AsSpan();
	//
	//		for(int i = 0; i < candidates.Length; i++)
	//		{
	//			ulong id = candidates[i];
	//			string docText = _documents[(int)id];
	//
	//			if(docText.Length == querySpan.Length && MemoryExtensions.Equals(docText.AsSpan(), querySpan, StringComparison.OrdinalIgnoreCase))
	//			{
	//				docId = id;
	//				return true;
	//			}
	//		}
	//
	//		return false;
	//	}
}
