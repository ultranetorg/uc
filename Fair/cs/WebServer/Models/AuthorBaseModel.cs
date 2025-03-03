using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AuthorBaseModel
{
	[JsonPropertyOrder(-2)]
	public string Id { get; set; }

	[JsonPropertyOrder(-1)]
	public string Title { get; set; }

	public AuthorBaseModel(Author author)
	{
		Id = author.Id.ToString();
		Title = author.Title;
	}
}
