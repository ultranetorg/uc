namespace Uccs.Net;

public class PlaceTransactionsPpc : McvPpc<PlaceTransactionsPpr>
{
	public Transaction[]	Transactions {get; set;}

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		return new PlaceTransactionsPpr {Results = Peering.ProcessIncoming(Transactions)};
	}

	public override void Read(Reader reader)
	{
		Transactions = reader.ReadArray<Transaction>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Transactions);
	}
}

public class PlaceTransactionsPpr : Result
{
	public TransactionResult[] Results { get; set; }

	public override void Read(Reader reader)
	{
		Results = reader.ReadArray<TransactionResult>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Results);
	}
}

