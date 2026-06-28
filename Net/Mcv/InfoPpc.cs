namespace Uccs.Net;

public class InfoPpc : McvPpc<InfoPpr>
{
	public override Result Execute()
	{
		RequireGraph();		

		return	new InfoPpr
				{
					Tables = Mcv.Tables.ToDictionary(i => i.Id, i => i.Name),
					Assets = [	
								Asset.Spacetime,
								Asset.Energy(0, Node.Mcv.LastConfirmedRound.ConsensusTime.Years),
								Asset.Energy(1, (byte)(Node.Mcv.LastConfirmedRound.ConsensusTime.Years + 1))
							]

				};
	}
}

public class InfoPpr : Result
{
	public Dictionary<byte, string>		Tables { get; set; }
	public Asset[]						Assets { get; set; }
}
