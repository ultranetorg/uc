using System.Diagnostics;

namespace Uccs.Net;

public class DownloadRoundsPpc : McvPpc<DownloadRoundsPpr>
{
	public int From { get; set; }
	public int To { get; set; }
	
	public override Result Execute()
	{
		RequireGraph();

		lock(Mcv.Lock)
		{
			
			if(Mcv.LastNonEmptyRound == null)	
				throw new NodeException(NodeError.TooEearly);

			if(From > Mcv.LastConfirmedRound.Id || To - From > Mcv.Net.P)
				throw new RequestException(RequestError.OutOfRange);

			var s = new MemoryStream();
			var w = new Writer(s, Peering.Constructor);
		
			var rs = Enumerable.Range(From, To - From + 1).Select(Mcv.FindRound).Where(i => i != null && i.Confirmed);

			w.Write(rs, i => w.WriteBytes(i.Raw));

			return	new DownloadRoundsPpr 
					{	
						LastNonEmptyRound	= Mcv.LastNonEmptyRound.Id,
						LastConfirmedRound	= Mcv.LastConfirmedRound.Id,
						BaseHash			= Mcv.GraphHash,
						Rounds				= [..rs.Select(i => i.Raw)]
					};
		}
	}
}

public class DownloadRoundsPpr : Result
{
	public int			LastNonEmptyRound { get; set; }
	public int			LastConfirmedRound { get; set; }
	public byte[]		BaseHash{ get; set; }
	public byte[][]		Rounds { get; set; }

	public Round[] Read(Mcv mcv, Constructor constructor)
	{
		if(Rounds == null)
			return [];

		return [..Rounds.Select(i =>	{
											var r = mcv.CreateRound();
											r.Restore(i);
											return r;
										})];
	}
}
