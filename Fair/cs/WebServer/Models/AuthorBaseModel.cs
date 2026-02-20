using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AuthorBaseModel
{
	[JsonPropertyOrder(-3)]
	public string Id { get; set; }

	[JsonPropertyOrder(-2)]
	public string Nickname { get; set; }

	[JsonPropertyOrder(-1)]
	public string Title { get; set; }

	public AuthorBaseModel(Author author)
	{
		Id = author.Id.ToString();
		Nickname = author.Name;
		Title = author.Title;
	}
}
