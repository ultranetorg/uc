namespace Uccs.Fair;

public class PublicationStatusChangeModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }

	public PublicationStatus Status { get; set; }

	public PublicationStatusChangeModel(PublicationStatusChange operation)
	{
		PublicationId = operation.Publication.ToString();
		Status = operation.Status;
	}
}
