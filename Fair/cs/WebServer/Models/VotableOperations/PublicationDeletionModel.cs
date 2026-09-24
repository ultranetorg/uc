namespace Uccs.Fair;

public class PublicationDeletionModel(PublicationDeletion operation) : BaseVotableOperationModel(operation)
{
	public AutoId PublicationId { get; set; } = operation.Publication;
}
