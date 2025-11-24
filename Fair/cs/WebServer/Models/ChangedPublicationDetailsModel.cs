namespace Uccs.Fair;

public class ChangedPublicationDetailsModel(string publicationId, Product product, int publicationVersion, FairAccount account, Category category, byte[]? logo)
	: ChangedPublicationModel(publicationId, product, publicationVersion, account, category, logo)
{
	public IEnumerable<ProductFieldValueModel> From { get; set; }
	public IEnumerable<ProductFieldValueModel> To { get; set; }
}
