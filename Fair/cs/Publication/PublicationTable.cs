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

		Mcv.PublicationTitles.StartExecution(e);

		foreach(var i in GraphEntities)
		{
			var c = e.FindCategory(i.Category);
			var r = Mcv.Products.Find(i.Product);
			var f = i.Fields.FirstOrDefault(f => f.Name == ProductField.Title);

			if(f != null)
			{
				var t = r.Get(f);

				e.PublicationTitles.Index(c.Site, i.Id, t.AsUtf8);
			}
		}
	
		Mcv.PublicationTitles.Dissolve(batch, e.PublicationTitles.Affected.Values, null);
	}
}
