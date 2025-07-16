namespace Uccs.Fair;

public class ModeratorPublicationModel(Publication publication, Category category, Product product, Author author)
{
	public string Id { get; set; } = publication.Id.ToString();

	public PublicationFlags Flags { get; set; } = publication.Flags;

	public string CategoryId { get; set; } = category.Id.ToString();
	public string CategoryTitle { get; set; } = category.Title;

	/// Creator field on Publication.
	public string CreatorId { get; set; } = publication.Creator.ToString();

	/// Product field on Publication.
	public string ProductId { get; set; } = product.Id.ToString();
	public int ProductUpdated { get; set; } = product.Updated.Days;

	public string AuthorId { get; set; } = author.Id.ToString();
	public string AuthorTitle { get; set; } = author.Title;
}
