using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationTable : Table<AutoId, Publication>
{
	public override string			Name => FairTable.Publication.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public PublicationTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Publication Create()
	{
		return new Publication(Mcv);
	}
}

public class PublicationExecution : TableExecution<AutoId, Publication>
{
	new FairExecution Execution => base.Execution as FairExecution;

	public PublicationExecution(FairExecution execution) : base(execution.Mcv.Publications, execution)
	{
	}
 
	public Publication Create(Site site)
	{
		Execution.IncrementCount((int)FairMetaEntityType.PublicationsCount);

		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(site.Id.B, e);
		a.Reviews = [];
			
		return Affected[a.Id] = a;
	}

	public void Delete(AutoId id)
	{
		var p = Execution.Publications.Affect(id);
		var s = Execution.Sites.Affect(p.Site);

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
			Execution.ProductTitles.Deindex(p);

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
