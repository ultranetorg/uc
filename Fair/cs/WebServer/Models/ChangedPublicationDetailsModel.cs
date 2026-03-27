namespace Uccs.Fair;

public class ChangedPublicationDetailsModel(string publicationId, Product product, int publicationVersion, FairUser account, Category category, AutoId publicationImageId)
	: ChangedPublicationModel(publicationId, product, publicationVersion, account, category, publicationImageId)
{
	public IEnumerable<FieldValueModel> From { get; set; }
	public IEnumerable<FieldValueModel> To { get; set; }
}
