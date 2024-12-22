namespace Uccs.Net;

public class PlaceTransactionsRequest : McvPpc<PlaceTransactionsResponse>
{
	public Transaction[]	Transactions {get; set;}

	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			lock(Mcv.Lock)
			{	
				RequireMember();

				var acc = Peering.ProcessIncoming(Transactions).Select(i => i.Signature).ToArray();

				return new PlaceTransactionsResponse {Accepted = acc};
			}
		}
	}
}

public class PlaceTransactionsResponse : PeerResponse
{
	public byte[][] Accepted { get; set; }
}

