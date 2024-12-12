using System.Text;

namespace Uccs.Rdn;

public class DomainTable : Table<DomainEntry>
{
	public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();

	public int						KeyToH(string domain) => BucketBase.FromBytes(Encoding.UTF8.GetBytes(domain.PadRight(3, '\0'), 0, 3));

	public DomainTable(RdnMcv rds) : base(rds)
	{
	}

	public DomainEntry FindEntry(string key)
	{
		var bid = KeyToH(key);

		return FindBucket(bid)?.Entries.Find(i => i.Address == key);
	}
	
	protected override DomainEntry Create(int cid)
	{
		return new DomainEntry(Mcv) {Id = new EntityId {H = cid}};
	}
	
 	public DomainEntry Find(string name, int ridmax)
 	{
	//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
	//	throw new IntegrityException("maxrid works inside pool only");

 		foreach(var r in Tail.Where(i => i.Id <= ridmax))
 			if(r.AffectedDomains.TryGetValue(name, out DomainEntry v))
 				return v;
 		
 		return FindEntry(name);
 	}

	public DomainEntry Find(EntityId id, int ridmax)
	{
		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
		//	throw new IntegrityException("maxrid works inside pool only");

		foreach(var r in Tail.Where(i => i.Id <= ridmax))
		{
			var a = r.AffectedDomains.Values.FirstOrDefault(i => i.Id == id);
			
			if(a != null)
				return a;
		}

		return FindBucket(id.H)?.Entries.Find(i => i.Id.E == id.E);
	}
	
//  		public Resource FindResource(Ura resource, int ridmax)
//  		{
// 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
// 			//	throw new IntegrityException("maxrid works inside pool only");
// 
//  			foreach(var r in Tail.Where(i => i.Id <= ridmax))
// 				if(r.AffectedDomains.TryGetValue(resource.Domain, out DomainEntry a))
// 				{	
// 					var x = a.Resources?.FirstOrDefault(i => i.Address.Resource == resource.Resource);
// 					
// 					if(x != null)
//  						return x;
// 				}
//  		
//  			return FindEntry(resource.Domain)?.Resources?.FirstOrDefault(i => i.Address.Resource == resource.Resource);
//  		}
// 
// 		
//  		public Resource FindResource(ResourceId id, int ridmax)
//  		{
// 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
// 			//	throw new IntegrityException("maxrid works inside pool only");
// 
//  			foreach(var r in Tail.Where(i => i.Id <= ridmax))
// 			{
// 				var x = r.AffectedDomains.FirstOrDefault(i => i.Value.Id.Ci == id.Ci && i.Value.Id.Ei == id.Di).Value?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);
// 
// 				if(x != null)
// 					return x;
// 			}
//  		
//  			return FindEntry(new EntityId(id.Ci, id.Di))?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);
//  		}
}
