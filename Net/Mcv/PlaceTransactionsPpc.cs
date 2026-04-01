namespace Uccs.Net;

public class PlaceTransactionsPpc : McvPpc<PlaceTransactionsPpr>
{
	public Transaction[]	Transactions {get; set;}

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		lock(Mcv.Lock)
		{	
			return new PlaceTransactionsPpr {Results = Peering.ProcessIncoming(Transactions).ToArray()};
		}
	}
}

public class PlaceTransactionsPpr : Result
{
	public TransactionResult[] Results { get; set; }
}

