namespace Uccs.Fair;

public class DisputeModel
{
	public string Id { get; set; }

	public int YesCount { get; set; }
	public int NoCount { get; set; }
	public int AbsCount { get; set; }

	public int Expiration { get; set; }

	public string Text { get; set; }

	public BaseVotableOperationModel Proposal { get; set; }

	public DisputeModel(Dispute dispute)
	{
		Id = dispute.Id.ToString();
		YesCount = dispute.Yes.Count();
		NoCount = dispute.No.Count();
		AbsCount = dispute.Abs.Count();
		Expiration = dispute.Expirtaion.Days;
		Text = dispute.Text;
	}
}
