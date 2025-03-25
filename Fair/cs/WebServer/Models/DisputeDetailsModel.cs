namespace Uccs.Fair;

public class DisputeDetailsModel : DisputeModel
{
	public IEnumerable<string> Pros { get; set; }
	public IEnumerable<string> Cons { get; set; }

	public DisputeDetailsModel(Dispute dispute) : base(dispute)
	{
		Pros = dispute.Pros.Select(x => x.ToString());
		Cons = dispute.Cons.Select(x => x.ToString());
	}
}
