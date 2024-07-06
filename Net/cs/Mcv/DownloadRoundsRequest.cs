using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class DownloadRoundsRequest : McvCall<DownloadRoundsResponse>
	{
		public int From { get; set; }
		public int To { get; set; }
		
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
				
				if(Mcv.LastNonEmptyRound == null)	
					throw new NodeException(NodeError.TooEearly);

				if(From > Mcv.LastNonEmptyRound.Id || To - From > Mcv.P)
					throw new InvalidRequestException("Invalid params");

				var s = new MemoryStream();
				var w = new BinaryWriter(s);
			
				w.Write(Enumerable.Range(From, To - From + 1).Select(i => Mcv.FindRound(i)).Where(i => i != null && i.Confirmed), i => i.Write(w));
			
				return new DownloadRoundsResponse {	LastNonEmptyRound	= Mcv.LastNonEmptyRound.Id,
													LastConfirmedRound	= Mcv.LastConfirmedRound.Id,
													BaseHash			= Mcv.BaseHash,
													Rounds				= s.ToArray()};
			}
		}
	}
	
	public class DownloadRoundsResponse : PeerResponse
	{
		public int		LastNonEmptyRound { get; set; }
		public int		LastConfirmedRound { get; set; }
		public byte[]	BaseHash{ get; set; }
		public byte[]	Rounds { get; set; }

		public Round[] Read(Mcv mcv)
		{
			if(Rounds == null)
				return [];

			var rd = new BinaryReader(new MemoryStream(Rounds));

			return rd.ReadArray(() =>{
										var r = mcv.CreateRound();
										r.Read(rd);
										return r;
									});
		}
	}
}
