namespace Uccs.Fair;

public class UserProposalModel(Proposal proposal, FairAccount signer) : BaseProposal(proposal)
{
	public AccountBaseAvatarModel Signer { get; } = new(signer);
}
