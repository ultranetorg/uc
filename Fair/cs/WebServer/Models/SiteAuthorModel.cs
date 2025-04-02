namespace Uccs.Fair;

public class SiteAuthorModel : AuthorBaseModel
{
	public IEnumerable<string> OwnersIds { get; set; }

	public SiteAuthorModel(Author author) : base(author)
	{
		OwnersIds = author.Owners.Select(x => x.ToString());
	}
}
