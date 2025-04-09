namespace Uccs.Fair;

public class DisputeDetailsModel : DisputeModel
{
	public IEnumerable<string> Pros { get; set; }
	public IEnumerable<string> Cons { get; set; }
	public IEnumerable<string> Abs { get; set; }

	public DisputeDetailsModel(Dispute dispute) : base(dispute)
	{
		Pros = dispute.Yes.Select(x => x.ToString());
		Cons = dispute.No.Select(x => x.ToString());
		Abs = dispute.Abs.Select(x => x.ToString());
	}
}
