namespace Uccs.Fair;

public class PublicationCreationModel(PublicationCreation operation) : BaseVotableOperationModel(operation)
{
	public string ProductId { get; set; } = operation.Product.ToString();
}
