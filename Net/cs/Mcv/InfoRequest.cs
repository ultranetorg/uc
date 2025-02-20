namespace Uccs.Net;

public class InfoRequest : McvPpc<InfoResponse>
{
	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			RequireBase();		

			return new InfoResponse {Tables = Mcv.Tables.ToDictionary(i => i.Name, i => i.Id)};
		}
	}
}

public class InfoResponse : PeerResponse
{
	public Dictionary<string, byte> Tables { get; set; }
}
