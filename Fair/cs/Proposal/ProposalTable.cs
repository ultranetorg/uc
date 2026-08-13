namespace Uccs.Fair;

public class ProposalTable : Table<AutoId, Proposal>
{
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ProposalTable(FairMcv mcv) : base(mcv, FairTable.Proposal.ToString())
	{
	}
	
	public override Proposal Create()
	{
		return new Proposal(Mcv);
	}
}
public class ProposalExecution : TableExecution<AutoId, Proposal, ProposalTable>
{
	new FairExecution Execution => base.Execution as FairExecution;

	public ProposalExecution(FairExecution execution) : base(execution.Mcv.Proposals, execution)
	{
	}

	public Proposal Create(Store store)
	{
		Execution.IncrementMetaInt(FairMetaEntityType.ProposalCount);

		var a = Table.Create();

		a.Id = LastCreatedId = new AutoId(Execution.IncrementMetaInt(FairMetaEntityType.ProposalIdCounter));
		a.Neither			= [];
		a.Any				= [];
		a.Ban				= [];
		a.Banish			= [];
		a.Any				= [];
		a.Comments			= [];

		LastCreatedId = a.Id;

		return Affected[a.Id] = a;
	}

	public void Delete(Store store, Proposal proposal)
	{
 		proposal.Deleted = true;
 		store.Proposals = store.Proposals.Remove(proposal.Id);

		foreach(var i in proposal.Comments)
			Execution.ProposalComments.Affect(i).Deleted = true;

	}

}