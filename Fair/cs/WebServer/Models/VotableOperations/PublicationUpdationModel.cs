namespace Uccs.Fair;

public class PublicationUpdationModel(PublicationUpdation operation) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();

	public int Version { get; set; } = operation.Version;
}
