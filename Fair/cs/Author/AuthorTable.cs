namespace Uccs.Fair;

public class AuthorTable : Table<AuthorEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();

	public AuthorTable(FairMcv rds) : base(rds)
	{
	}
		
	public override AuthorEntry Create()
	{
		return new AuthorEntry(Mcv);
	}
}

