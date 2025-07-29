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

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in GraphEntities)
		{
			//var c = e.Categories.Find(i.Category);
			var r = Mcv.Products.Find(i.Product);
			var f = r.Versions[i.ProductVersion].Fields.FirstOrDefault(f => f.Name == ProductFieldName.Title);

			if(f != null)
			{
				e.PublicationTitles.Index(i.Site, i.Id, f.AsUtf8);
			}
		}
			
		Mcv.PublicationTitles.Commit(batch, e.PublicationTitles.Affected.Values, e.PublicationTitles, null);
		(lastincommit as FairRound).PublicationTitles = new (Mcv.PublicationTitles) { EntryPoints = e.PublicationTitles.EntryPoints};
	}

	public SearchResult[] Search(AutoId site, string query, int skip, int take)
	{
		var result = Mcv.PublicationTitles.Search(	query.ToLowerInvariant(), 
													skip, 
													take, 
													i => i.References.ContainsKey(site),
													Mcv.PublicationTitles.Latest, 
													(Mcv.LastConfirmedRound as FairRound).PublicationTitles.EntryPoints);

		return result.Select(i =>	{
										return new SearchResult {Entity = i.References[site], Text = i.Text};
																								 
									}).ToArray();
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
		p.Deleted = true;

 		var c = Execution.Categories.Find(p.Category);
		var s = Execution.Sites.Affect(c.Site);
		var r = Execution.Products.Affect(p.Product);
		//var a = execution.Authors.Find(.Author);

		r.Publications = r.Publications.Remove(r.Id);

		if(c.Publications.Contains(p.Id))
		{
			c = Execution.Categories.Affect(c.Id);
			c.Publications = c.Publications.Remove(id);

			s.PublicationsCount--;
		}
		
		if(s.UnpublishedPublications.Contains(p.Id))
		{
			s.UnpublishedPublications = s.UnpublishedPublications.Remove(p.Id);
		}

		foreach(var i in p.Reviews)
		{
			Execution.Reviews.Delete(s, i);
		}
		
		var f = r.Versions[p.ProductVersion].Fields.FirstOrDefault(f => f.Name == ProductFieldName.Title);
		
		if(f != null)
		{
			Execution.PublicationTitles.Deindex(c.Site, f.AsUtf8);
		}

		Execution.Free(s, s, Execution.Net.EntityLength);
	}
}
