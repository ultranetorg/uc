using System.Linq;

namespace Uccs.Net
{
	public class AnalysisTable : Table<AnalysisEntry, byte[]>
	{
		protected override bool	Equal(byte[] a, byte[] b) => a.SequenceEqual(b);

		public  AnalysisTable(Mcv chain) : base(chain)
		{
		}
		
		protected override  AnalysisEntry Create()
		{
			return new  AnalysisEntry(Mcv);
		}

		protected override byte[] KeyToBytes(byte[] key)
		{
			return key;
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
