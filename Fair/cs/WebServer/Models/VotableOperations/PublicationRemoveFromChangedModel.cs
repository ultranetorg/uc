namespace Uccs.Fair;

public class PublicationRemoveFromChangedModel(PublicationRemoveFromChanged operation) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();
}
