namespace Uccs.Fair;

public class AuthorModel(Author author) : AuthorBaseModel(author)
{
	public string OwnerId { get; set; }
}
