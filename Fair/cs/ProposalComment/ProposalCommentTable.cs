namespace Uccs.Fair;

public class ProposalCommentTable : Table<AutoId, ProposalComment>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ProposalCommentTable(FairMcv mcv) : base(mcv, FairTable.ProposalComment.ToString())
	{
	}
	
	public override ProposalComment Create()
	{
		return new ProposalComment(Mcv);
	}
 }

public class ProposalCommentExecution : TableExecution<AutoId, ProposalComment>
{
	public ProposalCommentExecution(FairExecution execution) : base(execution.Mcv.ProposalComments, execution)
	{
	}

	public ProposalComment Create(Proposal proposal)
	{
		Execution.IncrementCount((int)FairMetaEntityType.ProposalCommentsCount);

		int e = Execution.GetNextEid(Table, proposal.Id.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(proposal.Id.B, e);

		return Affected[a.Id] = a;
	}
}
