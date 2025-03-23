namespace Uccs.Fair;

public class AuthorTable : Table<Author>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();

	public AuthorTable(FairMcv rds) : base(rds)
	{
	}
		
	public override Author Create()
	{
		return new Author(Mcv);
	}
}

