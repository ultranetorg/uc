namespace Uccs.Fair;

public class PublicationUpdationModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }
	public ProductFieldVersionReferenceModel Change { get; set; }
	//public bool Resolution { get; set; }

	public PublicationUpdationModel(PublicationUpdation operation)
	{
		PublicationId = operation.Publication.ToString();
		Change = new ProductFieldVersionReferenceModel(operation.Change);
		//Resolution = operation.Resolution;
	}
}
