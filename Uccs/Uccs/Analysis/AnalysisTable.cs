using System;
using System.Linq;

namespace Uccs.Net
{
	public class AnalysisTable : Table<AnalysisEntry, byte[]>
	{
		public override bool		Equal(byte[] a, byte[] b) => a.SequenceEqual(b);
		public override Span<byte>	KeyToCluster(byte[] address) => new Span<byte>(address, 0, Cluster.IdLength);

		public  AnalysisTable(Mcv chain) : base(chain)
		{
		}
		
		protected override  AnalysisEntry Create()
		{
			return new  AnalysisEntry(Mcv);
		}
		
 		public AnalysisEntry Find(byte[] release, int ridmax)
 		{
 			foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
 				if(r.AffectedAnalyses.TryGetValue(release, out AnalysisEntry v))
 					return v;
 		
 			return FindEntry(release);
 		}
	}
}
