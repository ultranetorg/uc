using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationTable : Table<AutoId, Publication>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public PublicationTable(FairMcv mcv) : base(mcv, FairTable.Publication.ToString())
	{
	}
	
	public override Publication Create()
	{
		return new Publication(Mcv);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in Mcv.Publications.GraphEntities)
		{
			if(i.IsPublished)
				e.PublicationTitles.Index(i);
		}
		
		Mcv.PublicationTitles.Commit(batch, e.PublicationTitles.Affected.Values, null, lastincommit);
	}
}

public class PublicationExecution : TableExecution<AutoId, Publication, PublicationTable>
{
	new FairExecution Execution => base.Execution as FairExecution;

	public PublicationExecution(FairExecution execution) : base(execution.Mcv.Publications, execution)
	{
	}
 
	public Publication Create(Store store)
	{
		Execution.IncrementMetaInt(FairMetaEntityType.PublicationsCount);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(Execution.IncrementMetaInt(FairMetaEntityType.PublicationsIdCounter));
		a.Reviews = [];
			
		return Affected[a.Id] = a;
	}

	public void Delete(AutoId id)
	{
		var p = Execution.Publications.Affect(id);
		var s = Execution.Stores.Affect(p.Store);

		foreach(var i in p.Reviews)
		{
			Execution.Reviews.Delete(s, i);
		}

		if(p.Category != null)
		{
			var c = Execution.Categories.Affect(p.Category);
			c.Publications = c.Publications.Remove(id);

			s.PublicationsCount--;
		}

		var r = Execution.Products.Affect(p.Product);
		
		r.Publications = r.Publications.Remove(id);
		s.UnpublishedPublications = s.UnpublishedPublications.Remove(id);
		
		var v = r.Versions.First(i => i.Id == p.ProductVersion);

		r.Versions = r.Versions.Replace(v, new ProductVersion {Id = v.Id, Fields = v.Fields, Refs = v.Refs - 1});
		
		if(p.IsPublished)
			Execution.PublicationTitles.Deindex(p);

		if(p.Flags.HasFlag(PublicationFlags.RequestedByAuthor))
		{ 
			var a = Execution.Authors.Affect(r.Author);
			Execution.Free(a, a, Execution.Net.EntityLength);
		}
		else
			Execution.Free(s, s, Execution.Net.EntityLength);

		p.Deleted = true;
	}
}
