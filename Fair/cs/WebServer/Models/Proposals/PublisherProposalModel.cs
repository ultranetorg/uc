namespace Uccs.Fair;

public class PublisherProposalModel(Proposal proposal, FairUser by, IEnumerable<AuthorBaseAvatarModel> authors) : ProposalModel(proposal, by)
{
	public IEnumerable<AuthorBaseAvatarModel> Authors { get; } = authors;
}
