namespace Uccs.Fair;

public class PublicationProductChangeModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }

	public string ProductId { get; set; }

	public PublicationProductChangeModel(PublicationProductChange opeartion)
	{
		PublicationId = opeartion.Publication.ToString();
		ProductId = opeartion.Product.ToString();
	}
}
