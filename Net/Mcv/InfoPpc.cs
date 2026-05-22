namespace Uccs.Net;

public class InfoPpc : McvPpc<InfoPpr>
{
	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();		

			return	new InfoPpr
					{
						Tables = Mcv.Tables.ToDictionary(i => i.Name, i => i.Id),
						Assets = [	
									Asset.Spacetime(),
									Asset.Energy(Node.Mcv.LastConfirmedRound.ConsensusTime.Years),
									Asset.Energy((byte)(Node.Mcv.LastConfirmedRound.ConsensusTime.Years + 1))
								 ]

					};
		}
	}
}

public class InfoPpr : Result
{
	public Dictionary<string, byte>		Tables { get; set; }
	public Asset[]						Assets { get; set; }
}
