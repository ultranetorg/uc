namespace Uccs.Fair;

public class DisputeModel
{
	public string Id { get; set; }

	public string SiteId { get; set; }

	public DisputeFlags Flags { get; set; }

	public ProposalModel Proposal { get; set; }

	public string[] Pros { get; set; }
	public string[] Cons { get; set; }

	public int Expirtaion { get; set; }

	public DisputeModel(Dispute dispute)
	{
		Id = dispute.Id.ToString();
		SiteId = dispute.Site.ToString();
		Flags = dispute.Flags;
		Proposal = new ProposalModel(dispute.Proposal);
		Expirtaion = dispute.Expirtaion.Days;
	}
}
