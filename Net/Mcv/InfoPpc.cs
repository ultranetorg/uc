namespace Uccs.Net;

public class InfoPpc : McvPpc<InfoPpr>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();		

			return new InfoPpr {Tables = Mcv.Tables.ToDictionary(i => i.Name, i => i.Id)};
		}
	}
}

public class InfoPpr : PeerResponse
{
	public Dictionary<string, byte> Tables { get; set; }
}
