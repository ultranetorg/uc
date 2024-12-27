namespace Uccs.Fair;

public class CatalogueTable : Table<CatalogueEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public CatalogueTable(FairMcv rds) : base(rds)
	{
	}
	
	public override CatalogueEntry Create()
	{
		return new CatalogueEntry(Mcv);
	}

	public CatalogueEntry Find(EntityId id, int ridmax)
	{
		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
		//	throw new IntegrityException("maxrid works inside pool only");

  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedCatalogues.TryGetValue(id, out var r))
    			return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		//if(e == null)
		//	return null;

		//e.Address.Publisher = Mcv.Publishers.Find(id, ridmax).Address;

		return e;
	}
	
// 	public CatalogueEntry Find(Ura address, int ridmax)
// 	{
//  		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
//  		//	throw new IntegrityException("maxrid works inside pool only");
// 
//   		foreach(var r in Tail.Where(i => i.Id <= ridmax))
//  		{	
//  			var x = r.AffectedCatalogues.Values.FirstOrDefault(i => i.Address == address);
//  					
//  			if(x != null)
//   				return x;
//  		}
//  
//         var d = Mcv.Publishers.Find(address.Publisher, ridmax);
// 
//         if(d == null)
//             return null;
// 
//   		var e = FindBucket(d.Id.H)?.Entries.Find(i => i.Id.E == d.Id.E && i.Address.Catalogue == address.Catalogue);
// 
// 		if(e == null)
// 			return null;
// 
// 		e.Address.Publisher = d.Address;
// 
// 		return e;
// 	}
 }
