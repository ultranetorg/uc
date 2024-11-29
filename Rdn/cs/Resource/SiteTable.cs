using System.Text;

namespace Uccs.Rdn
{
	public class SiteTable : Table<SiteEntry>
	{
		public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();
		public new RdnMcv				Mcv => base.Mcv as RdnMcv;

		//public ushort					KeyToCluster(string domain) => Cluster.FromBytes(Encoding.UTF8.GetBytes(domain, 0, sizeof(ushort)));

		public SiteTable(RdnMcv rds) : base(rds)
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
		
		protected override SiteEntry Create()
		{
			return new SiteEntry(Mcv);
		}

		public SiteEntry Find(EntityId id, int ridmax)
		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Tail.Where(i => i.Id <= ridmax))
			{
				var a = r.AffectedSites.Values.FirstOrDefault(i => i.Id == id);
				
				if(a != null)
					return a;
			}

			return FindEntry(id);
		}
		
  		public Resource FindResource(Ura resource, int ridmax)
  		{
 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
 			//	throw new IntegrityException("maxrid works inside pool only");
 
            var d = Mcv.Domains.Find(resource.Domain, ridmax);

            if(d == null)
                return null;

  			foreach(var r in Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedSites.TryGetValue(d.Id, out SiteEntry a))
 				{	
 					var x = a.Resources?.FirstOrDefault(i => i.Address.Resource == resource.Resource);
 					
 					if(x != null)
  						return x;
 				}
  		
  			return FindEntry(d.Id)?.Resources?.FirstOrDefault(i => i.Address.Resource == resource.Resource);
  		}
 
 		
  		public Resource FindResource(ResourceId id, int ridmax)
  		{
 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
 			//	throw new IntegrityException("maxrid works inside pool only");
 
  			foreach(var r in Tail.Where(i => i.Id <= ridmax))
 			{
 				if(r.AffectedSites.TryGetValue(id.DomainId, out var s))
                {
                    var x = s.Resources.FirstOrDefault(i => i.Id.Ri == id.Ri);
 
 				    if(x != null)
 					    return x;
                }
 			}
  		
  			return FindEntry(new EntityId(id.Ci, id.Di))?.Resources.FirstOrDefault(i => i.Id.Ri == id.Ri);
  		}
	}
}
