using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public class PublicationTitleIndex : HnswTable<string, StringHnswEntity>
{
	public override bool			IsIndex => true;
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();

	public PublicationTitleIndex(Mcv mcv, int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int threshold = 100, int minDiversity = 100) : base(mcv, new NeedlemanWunsch(), maxLevel, maxConnections, efConstruction, threshold, minDiversity)
	{
	}

	public override StringHnswEntity Create()
	{
		return new StringHnswEntity();
	}

	public PublicationTitleExecution CreateExecuting(Execution execution)
	{
		return new PublicationTitleExecution(execution as FairExecution);
	}
}

public class PublicationTitleState : HnswTableState<string, StringHnswEntity>
{
	public PublicationTitleState(HnswTable<string, StringHnswEntity> table) : base(table)
	{
	}
}

public class PublicationTitleExecution : HnswTableExecution<string, StringHnswEntity>
{
	public PublicationTitleExecution(FairExecution execution) : base(execution, execution.Mcv.PublicationTitles)
	{
		EntryPoints = execution.Round.PublicationTitles?.EntryPoints ?? Table.EntryPoints;
	}

	public override StringHnswEntity Affect(HnswId id)
	{
 		if(Affected.TryGetValue(id, out var a))
 			return a;
 		
 		a = Table.Find(id, Execution.Round.Id);
 
 		if(a == null)
 		{
 			a = Table.Create();
 			a.Id = id;
 			a.Connections = [];
 			a.References = [];
 		
 			return Affected[id] = a;
 		} 
 		else
 		{
			a = a.Clone();

			var e = EntryPoints.Find(i => i.Id == a.Id);
			
			if(e != null)
			{
				AffectEntryPoints();
				EntryPoints.Remove(e);
				EntryPoints.Add(a);
			}

 			return Affected[id] = a;
 		}
	}

	public StringHnswEntity Find(string text)
 	{
		var e = Affected.Values.FirstOrDefault(i => i.Text == text);

 		if(e != null)
			if(!e.Deleted)
    			return e;
			else
				return null;

  		foreach(var i in Execution.Mcv.Tail.Where(i => i.Id <= Execution.Round.Id))
		{	
			e = i.FindState<PublicationTitleState>(Table).Affected.Values.FirstOrDefault(i => i.Text == text);
			if(e != null)
				if(!e.Deleted)
    				return e;
				else
					return null;
		}

 		var x = Encoding.UTF8.GetBytes(text, 0, Math.Min(text.Length, 32));
 		var b = HnswId.ToBucket(RandomLevel(Cryptography.Hash(2, x)), x);
 		
		e = Table.FindBucket(b)?.Entries.Find(i => i.Text == text);

		if(e != null)
			if(!e.Deleted)
    			return e;
			else
				return null;

		return null;
 	}

 	public void Index(AutoId site, AutoId entity, string text)
 	{
		text = text.ToLowerInvariant();

		var e =	Find(text);

 		if(e == null || Table.Metric.ComputeDistance(e.Text, text) != 0)
 		{
 			var x = Encoding.UTF8.GetBytes(text, 0, Math.Min(text.Length, 32));
 			var b = HnswId.ToBucket(RandomLevel(Cryptography.Hash(2, x)), x);

 			var id = new HnswId(b, Execution.GetNextEid(Table, b));
 	
 			e = Affect(id);
 	
 			e.Text = text;
			//e.Hash = Metric.Hashify(text);
 			
 			Add(e);
 		}
 		else
			e = Affect(e.Id);

 		if(!e.References.ContainsKey(site))
 		{	
 			e.References = new (e.References);
 			e.References[site] = entity;
 		}
 	}

 	public void Deindex(AutoId site, Publication publication, string text)
 	{
		text = text.ToLowerInvariant();

 		var e =	Find(text);
	
		e = Affect(e.Id);
	
		e.References = new (e.References);
		e.References.Remove(site);
 	}
}
