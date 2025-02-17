using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AuthorBaseModel
{
	[JsonPropertyOrder(-4)]
	public string Id { get; set; }

	[JsonPropertyOrder(-3)]
	public string Title { get; set; }

	[JsonPropertyOrder(-2)]
	public short Expiration { get; set; }

	[JsonPropertyOrder(-1)]
	public long Space { get; set; }

	public AuthorBaseModel(Author author)
	{
		Id = author.Id.ToString();
		Title = author.Title;
		Expiration = author.Expiration;
		Space = author.Space;
	}
}
