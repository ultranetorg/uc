namespace Uccs.Fair;

public class PublicationStatusChangeModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }

	public PublicationStatus Status { get; set; }

	public PublicationStatusChangeModel(PublicationApproval operation)
	{
		PublicationId = operation.Publication.ToString();
		throw new NotImplementedException(); ///Status = operation.Status;
	}
}
