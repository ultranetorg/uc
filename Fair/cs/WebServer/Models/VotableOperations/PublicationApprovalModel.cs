namespace Uccs.Fair;

public class PublicationApprovalModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }

	public PublicationApprovalModel(PublicationApproval operation)
	{
		PublicationId = operation.Publication.ToString();
	}
}
