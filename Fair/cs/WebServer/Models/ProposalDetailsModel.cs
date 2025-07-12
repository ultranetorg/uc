namespace Uccs.Fair;

public class ProposalDetailsModel : ProposalModel
{
	public IEnumerable<string> Pros { get; set; }
	public IEnumerable<string> Cons { get; set; }
	public IEnumerable<string> Abs { get; set; }

	public ProposalDetailsModel(Proposal dispute) : base(dispute)
	{
		Pros = dispute.Yes.Select(x => x.ToString());
		Cons = dispute.No.Select(x => x.ToString());
		Abs = dispute.Abs.Select(x => x.ToString());
	}
}
