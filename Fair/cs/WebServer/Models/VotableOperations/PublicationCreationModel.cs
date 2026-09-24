namespace Uccs.Fair;

public class PublicationCreationModel(PublicationCreation operation) : BaseVotableOperationModel(operation)
{
	public AutoId ProductId { get; set; } = operation.Product;
}
