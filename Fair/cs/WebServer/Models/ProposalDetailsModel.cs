namespace Uccs.Fair;

public class ProposalDetailsModel(Proposal dispute, FairAccount account) : ProposalModel(dispute, account)
{
	public IEnumerable<string> Pros { get; set; } = dispute.Yes.Select(x => x.ToString());
	public IEnumerable<string> Cons { get; set; } = dispute.No.Select(x => x.ToString());
	public IEnumerable<string> Abs { get; set; } = dispute.Abs.Select(x => x.ToString());
}
