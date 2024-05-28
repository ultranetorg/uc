using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class DownloadRoundsRequest : RdcCall<DownloadRoundsResponse>
	{
		public Guid McvGuid { get; set; } ///
		public int From { get; set; }
		public int To { get; set; }
		
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
				
				if(sun.Mcv.LastNonEmptyRound == null)	
					throw new NodeException(NodeError.TooEearly);

				if(From > sun.Mcv.LastNonEmptyRound.Id || To - From > Mcv.P)
					throw new InvalidRequestException("Invalid params");

				var s = new MemoryStream();
				var w = new BinaryWriter(s);
			
				/// USE McvGuid
				McvGuid = McvGuid;

				w.Write(Enumerable.Range(From, To - From + 1).Select(i => sun.Mcv.FindRound(i)).Where(i => i != null && i.Confirmed), i => i.Write(w));
			
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
