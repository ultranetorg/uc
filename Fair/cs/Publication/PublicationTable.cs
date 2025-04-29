using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationTable : Table<Publication>
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

		foreach(var cl in Clusters)
			foreach(var b in cl.Buckets)
				foreach(var i in b.Entries)
				{
					var c = e.FindCategory(i.Category);
					var r = Mcv.Products.Find(i.Product);
					var f = i.Fields.FirstOrDefault(f => f.Name == ProductField.Title);

					if(f != null)
					{
						var t = r.Get(f);

						var w = e.AffectPublicationTitle(new RawId(t));
	
						// w.References[c.Site] = [..w.References[c.Site], i.Id];
					}
				}
	
		Mcv.Words.Save(batch, e.AffectedWords.Values, null);
	}
}
