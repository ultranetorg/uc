using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class DomainTable : Table<DomainEntry, string>
	{
		public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();

		public override bool			Equal(string a, string b) => a.Equals(b);
		public override Span<byte>		KeyToCluster(string domain) => new Span<byte>(Encoding.UTF8.GetBytes(domain, 0, ClusterBase.IdLength));

		public DomainTable(RdnMcv rds) : base(rds)
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
				var x = r.AffectedDomains.FirstOrDefault(i => i.Value.Id.Ci.SequenceEqual(id.Ci) && i.Value.Id.Ei == id.Di).Value?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);

				if(x != null)
					return x;
			}
 		
 			return FindEntry(new EntityId(id.Ci, id.Di))?.Resources?.FirstOrDefault(i => i.Id.Ri == id.Ri);
 		}
	}
}
