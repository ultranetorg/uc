using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class DomainTable : Table<DomainEntry, string>
	{
		public override bool		Equal(string a, string b) => a.Equals(b);
		public override Span<byte>	KeyToCluster(string domain) => new Span<byte>(Encoding.UTF8.GetBytes(domain, 0, Cluster.IdLength));

		public DomainTable(Mcv chain) : base(chain)
		{
		}
		
		protected override DomainEntry Create()
		{
			return new DomainEntry(Mcv);
		}
		
 		public DomainEntry Find(string name, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedDomains.TryGetValue(name, out DomainEntry v))
 					return v;
 		
 			return FindEntry(name);
 		}
		
 		public Resource FindResource(ResourceAddress resource, int ridmax)
 		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
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

 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
			{
				var x = r.AffectedDomains.FirstOrDefault(i => i.Value.Id.Ci.SequenceEqual(id.Ci) && i.Value.Id.Ei == id.Ai).Value?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);

				if(x != null)
					return x;
			}
 		
 			return FindEntry(new EntityId(id.Ci, id.Ai))?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);
 		}
	}
}
