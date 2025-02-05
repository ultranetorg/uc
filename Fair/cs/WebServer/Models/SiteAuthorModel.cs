namespace Uccs.Fair;

public class SiteAuthorModel(Author author) : AuthorBaseModel(author)
{
	public string OwnerId { get; set; }

	public IEnumerable<PublicationBaseModel> Publications { get; set; }
}
