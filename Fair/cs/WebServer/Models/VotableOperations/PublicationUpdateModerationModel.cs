namespace Uccs.Fair;

public class PublicationUpdateModerationModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }
	public ProductFieldVersionReferenceModel Change { get; set; }
	public bool Resolution { get; set; }

	public PublicationUpdateModerationModel(PublicationUpdation operation)
	{
		PublicationId = operation.Publication.ToString();
		Change = new ProductFieldVersionReferenceModel(operation.Change);
		Resolution = operation.Resolution;
	}
}
