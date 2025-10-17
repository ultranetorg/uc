namespace Uccs.Fair;

public class ProposalTable : Table<AutoId, Proposal>
{
	public override string			Name => FairTable.Proposal.ToString();
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ProposalTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Proposal Create()
	{
		return new Proposal(Mcv);
	}
}
public class ProposalExecution : TableExecution<AutoId, Proposal>
{
	new FairExecution Execution => base.Execution as FairExecution;

	public ProposalExecution(FairExecution execution) : base(execution.Mcv.Proposals, execution)
	{
	}

	public Proposal Create(Site site)
	{
		Execution.IncrementCount((int)FairMetaEntityType.ProposalCount);

		int e = Execution.GetNextEid(Table, site.Id.B);

		var a = Table.Create();
		a.Id = LastCreatedId = new AutoId(site.Id.B, e);
		a.Neither = [];
		a.Any = [];
		a.Ban = [];
		a.Banish = [];
		a.Any = [];
		a.Comments = [];

		LastCreatedId = a.Id;

		return Affected[a.Id] = a;
	}

	public void Delete(Site site, Proposal proposal)
	{
 		proposal.Deleted = true;
 		site.Proposals = site.Proposals.Remove(proposal.Id);

		foreach(var i in proposal.Comments)
			Execution.ProposalComments.Affect(i).Deleted = true;

	}

}