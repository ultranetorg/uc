namespace Uccs.Fair;

public class ChangedPublicationDetailsModel(string publicationId, Product product, int publicationVersion, FairAccount account, Category category, AutoId publicationImageId)
	: ChangedPublicationModel(publicationId, product, publicationVersion, account, category, publicationImageId)
{
	public IEnumerable<ProductFieldValueModel> From { get; set; }
	public IEnumerable<ProductFieldValueModel> To { get; set; }
}
