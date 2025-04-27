using System.Text;

namespace Uccs.Fair;

public class PublicationTitleHnswEntity : HnswNode
{
	public string								Text { get; set; }
	public SortedDictionary<EntityId, EntityId>	References { get; set; }

	public PublicationTitleHnswEntity Clone()
	{
		var a = new PublicationTitleHnswEntity()  {	Id			= Id,
													Hash		= Hash,
													Connections	= Connections,
													Text		= Text,
													References	= References};

		return a;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Text		= reader.ReadUtf8();
		References	= reader.ReadSortedDictionary(() => reader.Read<EntityId>(), () => reader.Read<EntityId>());
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


public class PublicationTitleIndex : HnswTable<string, PublicationTitleHnswEntity>
{
	public override bool			IsIndex => true;
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public PublicationTitleIndex(Mcv mcv, int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int threshold = 10, int minDiversity = 10) : base(mcv, new Simhash(), maxLevel, maxConnections, efConstruction, threshold, minDiversity)
	{
	}

	public override PublicationTitleHnswEntity Create()
	{
		return new PublicationTitleHnswEntity();
	}

	public override ulong Hashify(string data)
	{
		return Metric.Hashify(data.ToLowerInvariant());
	}

	public override PublicationTitleHnswEntity Affect(HnswId id, FairExecution execution)
	{
 		if(execution.AffectedPublicationTitles.TryGetValue(id, out var a))
 			return a;
 			
 		a = Mcv.PublicationTitles.Find(id, execution);
 
 		if(a == null)
 		{
 			a = Mcv.PublicationTitles.Create();
 			a.Id = id;
 			a.Connections = [];
 			a.References = [];
 		
 			return execution.AffectedPublicationTitles[id] = a;
 		} 
 		else
 		{
 			return execution.AffectedPublicationTitles[id] = a.Clone();
 		}
	}

 	public void Index(EntityId site, EntityId entity, string text, FairExecution execution)
 	{
		text = text.ToLowerInvariant();

 		var x = Encoding.UTF8.GetBytes(text, 0, Math.Min(text.Length, 32));
 		
 		var e = Search(text, 1, i => i.References.ContainsKey(site)).FirstOrDefault();
 
 		if(e == null)
 		{
 			var b = HnswId.ToBucket(RandomLevel(Cryptography.Hash(x)), x);
 	
 			var id = new HnswId(b, execution.GetNextEid(this, b));
 	
 			e = Affect(id, execution);
 	
 			e.Text = text;
			e.Hash = Metric.Hashify(text);
 			
 			Add(e, execution);
 		}
 		
 		if(!e.References.ContainsKey(site))
 		{	
 			e.References = new (e.References);
 			e.References[site] = entity;
 		}
 	}

 	public void Deindex(EntityId site, Publication publication, string text, FairExecution execution)
 	{
//		var r = Mcv.PublicationTitles.Search(text, 1, i => i.References.ContainsKey(site));
//	
//		var e = Affect(r[0].Id, execution);
//	
//		e.References = new (e.References);
//		e.References.Remove(site);
 	}
}
