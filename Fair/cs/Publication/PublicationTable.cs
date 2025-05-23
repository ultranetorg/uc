using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationTable : Table<AutoId, Publication>
{
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
			var c = e.Categories.Find(i.Category);
			var r = Mcv.Products.Find(i.Product);
			var f = i.Fields.FirstOrDefault(f => f.Name == ProductField.Title);

			if(f != null)
			{
				var t = r.Get(f);

				e.PublicationTitles.Index(c.Site, i.Id, t.AsUtf8);
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
	public PublicationExecution(FairExecution execution) : base(execution.Mcv.Publications, execution)
	{
	}
 
	public Publication Create(Site site)
	{
		Execution.IncrementCount((int)FairMetaEntityType.PublicationsCount);

		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = new AutoId(site.Id.B, e);
		a.Fields = [];
		a.Reviews = [];
		a.ReviewChanges = [];
			
		return Affected[a.Id] = a;
	}
}
