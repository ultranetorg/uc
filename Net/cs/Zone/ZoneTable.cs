using System;
using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
// 	public class ZoneTable : Table<ZoneEntry, Guid>
// 	{
// 		public override bool		Equal(Guid a, Guid b) => a.Equals(b);
// 		public override Span<byte>	KeyToCluster(Guid account) => new Span<byte>(account.ToByteArray(), 0, ClusterBase.IdLength);
// 
// 		public ZoneTable(Mcv chain) : base(chain)
// 		{
// 		}
// 
// 		protected override ZoneEntry Create()
// 		{
// 			return new ZoneEntry(Mcv);
// 		}
// 
// 		public ZoneEntry Find(Guid account, int ridmax)
// 		{
// 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
// 			//	throw new IntegrityException("maxrid works inside pool only");
// 
// 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
// 				if(r.AffectedZones.TryGetValue(account, out var e))
// 					return e;
// 
// 			return FindEntry(account);
// 		}
// 
// 		public ZoneEntry Find(EntityId account, int ridmax)
// 		{
// 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
// 			//	throw new IntegrityException("maxrid works inside pool only");
// 
// 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
// 			{
// 				var a = r.AffectedZones.Values.FirstOrDefault(i => i.Id == account);
// 				
// 				if(a != null)
// 					return a;
// 			}
// 
// 			return FindEntry(account);
// 		}
// 	}
}
