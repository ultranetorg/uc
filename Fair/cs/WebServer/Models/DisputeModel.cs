namespace Uccs.Fair;

public class DisputeModel
{
	public string Id { get; set; }

	// public string SiteId { get; set; }

	public DisputeFlags Flags { get; set; }

	public ProposalModel Proposal { get; set; }

	public IEnumerable<string> Pros { get; set; }
	public IEnumerable<string> Cons { get; set; }

	public int Expiration { get; set; }

	public DisputeModel(Dispute dispute)
	{
		Id = dispute.Id.ToString();
		// SiteId = dispute.Site.ToString();
		Flags = dispute.Flags;
		Proposal = new ProposalModel(dispute.Proposal);
		Pros = dispute.Pros.Select(x => x.ToString());
		Cons = dispute.Cons.Select(x => x.ToString());
		Expiration = dispute.Expirtaion.Days;
	}
}
