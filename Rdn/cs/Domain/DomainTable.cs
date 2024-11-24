using System.Text;

namespace Uccs.Rdn
{
	public class DomainTable : Table<DomainEntry>
	{
		public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();

		public ushort					KeyToCluster(string domain) => Cluster.FromBytes(Encoding.UTF8.GetBytes(domain, 0, sizeof(ushort)));

		public DomainTable(RdnMcv rds) : base(rds)
		{
		}

		public DomainEntry FindEntry(string key)
		{
			var cid = KeyToCluster(key);

			var c = _Clusters.Find(i => i.Id == cid);

			if(c == null)
				return null;

			var e = c.Entries.Find(i => i.Address == key);

			return e;
		}
		
		protected override DomainEntry Create()
		{
			return new DomainEntry(Mcv);
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

			return FindEntry(id);
		}
		
 		public Resource FindResource(Ura resource, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedDomains.TryGetValue(resource.Domain, out DomainEntry a))
				{	
					var x = a.Resources?.FirstOrDefault(i => i.Address.Resource == resource.Resource);
					
					if(x != null)
 						return x;
				}
 		
 			return FindEntry(resource.Domain)?.Resources?.FirstOrDefault(i => i.Address.Resource == resource.Resource);
 		}

		
 		public Resource FindResource(ResourceId id, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Tail.Where(i => i.Id <= ridmax))
			{
				var x = r.AffectedDomains.FirstOrDefault(i => i.Value.Id.Ci == id.Ci && i.Value.Id.Ei == id.Di).Value?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);

				if(x != null)
					return x;
			}
 		
 			return FindEntry(new EntityId(id.Ci, id.Di))?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);
 		}
	}
}
