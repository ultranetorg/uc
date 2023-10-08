using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class DownloadRoundsRequest : RdcRequest
	{
		public int From { get; set; }
		public int To { get; set; }
		
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
				if(sun.Mcv.LastNonEmptyRound == null)	throw new RdcNodeException(RdcNodeError.TooEearly);

				var s = new MemoryStream();
				var w = new BinaryWriter(s);
			
				w.Write(Enumerable.Range(From, To - From + 1).Select(i => sun.Mcv.FindRound(i)).Where(i => i != null), i => i.Write(w));
			
				return new DownloadRoundsResponse {	LastNonEmptyRound	= sun.Mcv.LastNonEmptyRound.Id,
													LastConfirmedRound	= sun.Mcv.LastConfirmedRound.Id,
													BaseHash			= sun.Mcv.BaseHash,
													Rounds				= s.ToArray()};
			}
		}
	}
	
	public class DownloadRoundsResponse : RdcResponse
	{
		public int		LastNonEmptyRound { get; set; }
		public int		LastConfirmedRound { get; set; }
		public byte[]	BaseHash{ get; set; }
		public byte[]	Rounds { get; set; }

		public Round[] Read(Mcv chain)
		{
			var rd = new BinaryReader(new MemoryStream(Rounds));

			return rd.ReadArray<Round>(() =>{
												var r = new Round(chain);
												r.Read(rd);
												return r;
											});
		}
	}
}
