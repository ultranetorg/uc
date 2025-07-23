namespace Uccs.Fair;

public class PublicationDeletionModel(PublicationDeletion operation) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();
}
