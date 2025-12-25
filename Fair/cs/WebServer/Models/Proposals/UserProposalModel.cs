namespace Uccs.Fair;

public class UserProposalModel(Proposal proposal, FairUser signer) : BaseProposal(proposal)
{
	public AccountBaseAvatarModel Signer { get; } = new(signer);
}
