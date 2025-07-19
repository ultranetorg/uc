using NativeImport;

namespace Uccs.Fair;

public class AuthorDetailsModel(Author author, byte[]? avatar) : AuthorBaseModel(author)
{
	public byte[] Avatar { get; set; } = avatar;

	public IEnumerable<string> OwnersIds { get; set; } = author.Owners.Select(x => x.ToString());
}
