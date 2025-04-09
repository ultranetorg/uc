namespace Uccs.Fair;

public class AuthorModel : AuthorBaseModel
{
	public IEnumerable<string> OwnersIds { get; set; }

	public AuthorModel(Author author) : base(author)
	{
		OwnersIds = author.Owners.Select(x => x.ToString());
	}
}
