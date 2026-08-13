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

public class ProposalCommentExecution : TableExecution<AutoId, ProposalComment, ProposalCommentTable>
{
	public ProposalCommentExecution(FairExecution execution) : base(execution.Mcv.ProposalComments, execution)
	{
	}

	public ProposalComment Create(Proposal proposal)
	{
		Execution.IncrementMetaInt(FairMetaEntityType.ProposalCommentsCount);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(Execution.IncrementMetaInt(FairMetaEntityType.ProposalCommentsIdCounter));

		return Affected[a.Id] = a;
	}
}
