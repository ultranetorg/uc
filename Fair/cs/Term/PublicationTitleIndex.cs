using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public class StringHnswEntity : HnswNode<string>
{
	public string								Text { get; set; }
	public override string						Data => Text;
	public SortedDictionary<AutoId, AutoId>	References { get; set; }

	public StringHnswEntity Clone()
	{
		var a = new StringHnswEntity() {Id			= Id,
										Connections	= Connections,
										Text		= Text,
										References	= References};

		return a;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Text		= reader.ReadUtf8();
		References	= reader.ReadSortedDictionary(reader.Read<AutoId>, reader.Read<AutoId>);
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.WriteUtf8(Text);
		writer.Write(References, i => writer.Write(i), i => writer.Write(i));
	}

	public override void ReadMain(BinaryReader r)
	{
		Read(r);
	}

	public override void WriteMain(BinaryWriter w)
	{
		Write(w);
	}

	public override void Cleanup(Round lastInCommit)
	{
	}
}


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

	public ExecutingPublicationTitleIndex CreateExecuting(Execution execution)
	{
		return new ExecutingPublicationTitleIndex(execution as FairExecution);
	}
}

public class ExecutingPublicationTitleIndex : ExecutingHnswTable<string, StringHnswEntity>
{
	public override PublicationTitleIndex		Table => Execution.Mcv.PublicationTitles;

	public ExecutingPublicationTitleIndex(FairExecution execution) : base(execution)
	{
		EntryPoints = execution.Round.PublicationTitlesEntryPoints ?? Table.EntryPoints;
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
			e = i.AffectedPublicationTitles.Values.FirstOrDefault(i => i.Text == text);
			if(e != null)
				if(!e.Deleted)
    				return e;
				else
					return null;
		}

 		var x = Encoding.UTF8.GetBytes(text, 0, Math.Min(text.Length, 32));
 		var b = HnswId.ToBucket(RandomLevel(Cryptography.Hash(x)), x);
 		
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
 			var b = HnswId.ToBucket(RandomLevel(Cryptography.Hash(x)), x);

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
