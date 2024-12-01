using System.Text;

namespace Uccs.Rdn
{
	public class ResourceTable : Table<ResourceEntry>
	{
		public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();
		public new RdnMcv				Mcv => base.Mcv as RdnMcv;

		//public ushort					KeyToCluster(string domain) => Cluster.FromBytes(Encoding.UTF8.GetBytes(domain, 0, sizeof(ushort)));

		public ResourceTable(RdnMcv rds) : base(rds)
		{
		}

// 		public DomainEntry FindEntry(EntityId key)
// 		{
// 			var cid = KeyToCluster(key);
// 
// 			var c = _Clusters.Find(i => i.Id == cid);
// 
// 			if(c == null)
// 				return null;
// 
// 			var e = c.Entries.Find(i => i.Address == key);
// 
// 			return e;
// 		}
		
		protected override ResourceEntry Create(ushort cid)
		{
			return new ResourceEntry(Mcv) {Id = new ResourceId {C = cid}};
		}

		public ResourceEntry Find(ResourceId id, int ridmax)
		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

  			foreach(var i in Tail.Where(i => i.Id <= ridmax))
				if(i.AffectedResources.TryGetValue(id, out var r))
    				return r;

			var e = FindCluster(id.C)?.Entries.Find(i => i.Id.E == id.E && i.Id.R == id.R);

			if(e == null)
				return null;

			e.Address.Domain = Mcv.Domains.Find(id, ridmax).Address;

			return e;
		}
		
  		public ResourceEntry Find(Ura address, int ridmax)
  		{
 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
 			//	throw new IntegrityException("maxrid works inside pool only");

  			foreach(var r in Tail.Where(i => i.Id <= ridmax))
 			{	
 				var x = r.AffectedResources.Values.FirstOrDefault(i => i.Address == address);
 					
 				if(x != null)
  					return x;
 			}
 
            var d = Mcv.Domains.Find(address.Domain, ridmax);

            if(d == null)
                return null;

  			var e = FindCluster(d.Id.C)?.Entries.Find(i => i.Id.E == d.Id.E && i.Address.Resource == address.Resource);

			if(e == null)
				return null;

			e.Address.Domain = d.Address;

			return e;
  		}
 
 		
//   		public Resource FindResource(ResourceId id, int ridmax)
//   		{
//  			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
//  			//	throw new IntegrityException("maxrid works inside pool only");
//  
//   			foreach(var r in Tail.Where(i => i.Id <= ridmax))
//  			{
//  				if(r.AffectedResources.TryGetValue(id.DomainId, out var s))
//                 {
//                     var x = s.Resources.FirstOrDefault(i => i.Id.Ri == id.Ri);
//  
//  				    if(x != null)
//  					    return x;
//                 }
//  			}
//   		
//   			return FindEntry(new EntityId(id.Ci, id.Di))?.Resources.FirstOrDefault(i => i.Id.Ri == id.Ri);
//   		}
	}
}
