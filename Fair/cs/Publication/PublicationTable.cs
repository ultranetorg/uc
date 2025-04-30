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

	public override void Index(WriteBatch batch)
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
	}
}

public class PublicationExecution : TableExecution<AutoId, Publication>
{
	public PublicationExecution(FairExecution execution) : base(execution.Mcv.Publications, execution)
	{
	}
 
	public Publication Create(Site site)
	{
		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = new AutoId(site.Id.B, e);
		a.Fields = [];
		a.Changes = [];
		a.Reviews = [];
		a.ReviewChanges = [];
			
		return Affected[a.Id] = a;
	}

	public Publication Affect(AutoId id)
	{
		if(Affected.TryGetValue(id, out var a))
			return a;
			
		var e = Find(id);

		return Affected[id] = e.Clone();
	}
}
