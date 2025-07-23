using NativeImport;

namespace Uccs.Fair;

public class AuthorDetailsModel(Author author, byte[]? avatar) : AuthorBaseModel(author)
{
	public string Description { get; set; } = author.Description;

	public byte[] Avatar { get; set; } = avatar;

	public IEnumerable<string> OwnersIds { get; set; } = author.Owners.Select(x => x.ToString());

	public IEnumerable<string> Links { get; set; } = author.Links.Select(x => x.ToString());
}
