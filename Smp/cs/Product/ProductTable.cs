namespace Uccs.Smp;

public class ProductTable : Table<ProductEntry>
{
	public IEnumerable<SmpRound>	Tail => Mcv.Tail.Cast<SmpRound>();
	public new SmpMcv				Mcv => base.Mcv as SmpMcv;

	public ProductTable(SmpMcv rds) : base(rds)
	{
	}
	
	public override ProductEntry Create()
	{
		return new ProductEntry(Mcv);
	}

	public ProductEntry Find(EntityId id, int ridmax)
	{
		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
		//	throw new IntegrityException("maxrid works inside pool only");

  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedProducts.TryGetValue(id, out var r))
    			return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		//if(e == null)
		//	return null;

		//e.Address.Publisher = Mcv.Publishers.Find(id, ridmax).Address;

		return e;
	}
	
// 	public ProductEntry Find(Ura address, int ridmax)
// 	{
//  		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
//  		//	throw new IntegrityException("maxrid works inside pool only");
// 
//   		foreach(var r in Tail.Where(i => i.Id <= ridmax))
//  		{	
//  			var x = r.AffectedProducts.Values.FirstOrDefault(i => i.Address == address);
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
//   		var e = FindBucket(d.Id.H)?.Entries.Find(i => i.Id.E == d.Id.E && i.Address.Product == address.Product);
// 
// 		if(e == null)
// 			return null;
// 
// 		e.Address.Publisher = d.Address;
// 
// 		return e;
// 	}
 }
