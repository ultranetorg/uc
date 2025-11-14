namespace Uccs.Net;

public class PlaceTransactionsPpc : McvPpc<PlaceTransactionsPpr>
{
	public Transaction[]	Transactions {get; set;}

	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
			lock(Mcv.Lock)
			{	
				RequireMember();

				var acc = Peering.ProcessIncoming(Transactions).Select(i => i.Signature).ToArray();

				return new PlaceTransactionsPpr {Accepted = acc};
			}
	}
}

public class PlaceTransactionsPpr : PeerResponse
{
	public byte[][] Accepted { get; set; }
}

