namespace Uccs.Fair;

public class PublicationUpdationModel(PublicationUpdation operation) : BaseVotableOperationModel(operation)
{
	public AutoId PublicationId { get; set; } = operation.Publication;

	public int Version { get; set; } = operation.Version;
}
