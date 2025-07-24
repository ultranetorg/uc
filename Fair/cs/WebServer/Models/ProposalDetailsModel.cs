namespace Uccs.Fair;

public class ProposalDetailsModel(Proposal proposal, FairAccount account) : ProposalModel(proposal, account)
{
	/// TODO
	///public IEnumerable<string> Pros { get; set; } = proposal.Yes.Select(x => x.ToString());
	///public IEnumerable<string> Cons { get; set; } = proposal.No.Select(x => x.ToString());
	public IEnumerable<string> Abs { get; set; } = proposal.Abs.Select(x => x.ToString());
}
