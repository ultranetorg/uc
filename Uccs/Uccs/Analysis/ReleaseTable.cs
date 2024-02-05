using System;
using System.Linq;

namespace Uccs.Net
{
	public class ReleaseTable : Table<ReleaseEntry, ReleaseAddress>
	{
		public override bool		Equal(ReleaseAddress a, ReleaseAddress b) => a == b;
		public override Span<byte>	KeyToCluster(ReleaseAddress address) => new Span<byte>(address.Hash, 0, Cluster.IdLength);

		public ReleaseTable(Mcv chain) : base(chain)
		{
		}
		
		protected override ReleaseEntry Create()
		{
			return new ReleaseEntry(Mcv);
		}
		
 		public ReleaseEntry Find(ReleaseAddress release, int ridmax)
 		{
 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedReleases.TryGetValue(release, out ReleaseEntry v))
 					return v;
 		
 			return FindEntry(release);
 		}
	}
}
