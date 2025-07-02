namespace Uccs.Fair;

public class ReviewTable : Table<AutoId, Review>
{
	public override string			Name => FairTable.Review.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ReviewTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Review Create()
	{
		return new Review(Mcv);
	}
 }

public class ReviewExecution : TableExecution<AutoId, Review>
{
	public ReviewExecution(FairExecution execution) : base(execution.Mcv.Reviews, execution)
	{
	}

	public Review Create(Publication publication)
	{
		Execution.IncrementCount((int)FairMetaEntityType.ReviewsCount);

		int e = Execution.GetNextEid(Table, publication.Id.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(publication.Id.B, e);

		return Affected[a.Id] = a;
	}
}
