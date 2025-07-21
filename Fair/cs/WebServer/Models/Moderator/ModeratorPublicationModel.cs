namespace Uccs.Fair;

public class ModeratorPublicationModel
{
	public string Id { get; set; }

	public PublicationFlags Flags { get; set; }

	public string CategoryId { get; set; }
	public string CategoryTitle { get; set; }

	/// Creator field on Publication.
	public string CreatorId { get; set; }

	/// Product field on Publication.
	public string ProductId { get; set; }
	public int ProductUpdated { get; set; }

	public string AuthorId { get; set; }
	public string AuthorTitle { get; set; }

	//public ProductFieldVersionReference[] Fields { get; set; }
	//public ProductFieldVersionReference[] Changes { get; set; }
	//public EntityId[] Reviews { get; set; }
	//public EntityId[] ReviewChanges { get; set; }

	public ModeratorPublicationModel(Publication publication, Category category, Product product, Author author)
	{
		Id = publication.Id.ToString();
		Flags = publication.Flags;

		CategoryId = category.Id.ToString();
		CategoryTitle = category.Title;

		//CreatorId = publication.Creator.ToString();

		ProductId = product.Id.ToString();
		ProductUpdated = product.Updated.Days;

		AuthorId = author.Id.ToString();
		AuthorTitle = author.Title;
	}
}
