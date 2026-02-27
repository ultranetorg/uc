namespace Uccs.Net;

public class PlaceTransactionsPpc : McvPpc<PlaceTransactionsPpr>
{
	public Transaction[]	Transactions {get; set;}

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		lock(Peering.TransactingLock)
		{	
			var acc = Peering.ProcessIncoming(Transactions).Select(i => i.Signature).ToArray();

			return new PlaceTransactionsPpr {Accepted = acc};
		}
	}
}

public class PlaceTransactionsPpr : Result
{
	public byte[][] Accepted { get; set; }
}

