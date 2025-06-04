namespace Uccs.Fair;

public class DisputeCommentTable : Table<AutoId, DisputeComment>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public DisputeCommentTable(FairMcv rds) : base(rds)
	{
	}
	
	public override DisputeComment Create()
	{
		return new DisputeComment(Mcv);
	}
 }

public class DisputeCommentExecution : TableExecution<AutoId, DisputeComment>
{
	public DisputeCommentExecution(FairExecution execution) : base(execution.Mcv.DisputeComments, execution)
	{
	}

	public DisputeComment Create(Dispute dispute)
	{
		Execution.IncrementCount((int)FairMetaEntityType.DisputeCommentsCount);

		int e = Execution.GetNextEid(Table, dispute.Id.B);

		var a = Table.Create();
		a.Id = new AutoId(dispute.Id.B, e);

		return Affected[a.Id] = a;
	}
}
