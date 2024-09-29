namespace Uccs.Net
{
	// 	public class NetTable : Table<NetEntry, Guid>
	// 	{
	// 		public override bool		Equal(Guid a, Guid b) => a.Equals(b);
	// 		public override Span<byte>	KeyToCluster(Guid account) => new Span<byte>(account.ToByteArray(), 0, ClusterBase.IdLength);
	// 
	// 		public NetTable(Mcv chain) : base(chain)
	// 		{
	// 		}
	// 
	// 		protected override NetEntry Create()
	// 		{
	// 			return new NetEntry(Mcv);
	// 		}
	// 
	// 		public NetEntry Find(Guid account, int ridmax)
	// 		{
	// 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
	// 			//	throw new IntegrityException("maxrid works inside pool only");
	// 
	// 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
	// 				if(r.AffectedNets.TryGetValue(account, out var e))
	// 					return e;
	// 
	// 			return FindEntry(account);
	// 		}
	// 
	// 		public NetEntry Find(EntityId account, int ridmax)
	// 		{
	// 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
	// 			//	throw new IntegrityException("maxrid works inside pool only");
	// 
	// 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
	// 			{
	// 				var a = r.AffectedNets.Values.FirstOrDefault(i => i.Id == account);
	// 				
	// 				if(a != null)
	// 					return a;
	// 			}
	// 
	// 			return FindEntry(account);
	// 		}
	// 	}
}
