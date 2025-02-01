namespace Uccs.Fair;

public class SiteTable : Table<SiteEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public SiteTable(FairMcv rds) : base(rds)
	{
	}
	
	public override SiteEntry Create()
	{
		return new SiteEntry(Mcv);
	}

	public SiteEntry Find(EntityId id, int ridmax)
	{
		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
		//	throw new IntegrityException("maxrid works inside pool only");

  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedSites.TryGetValue(id, out var r))
    			return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		//if(e == null)
		//	return null;

		//e.Address.Publisher = Mcv.Publishers.Find(id, ridmax).Address;

		return e;
	}
	
// 	public SiteEntry Find(Ura address, int ridmax)
// 	{
//  		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
//  		//	throw new IntegrityException("maxrid works inside pool only");
// 
//   		foreach(var r in Tail.Where(i => i.Id <= ridmax))
//  		{	
//  			var x = r.AffectedSites.Values.FirstOrDefault(i => i.Address == address);
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
//   		var e = FindBucket(d.Id.H)?.Entries.Find(i => i.Id.E == d.Id.E && i.Address.Site == address.Site);
// 
// 		if(e == null)
// 			return null;
// 
// 		e.Address.Publisher = d.Address;
// 
// 		return e;
// 	}
 }
