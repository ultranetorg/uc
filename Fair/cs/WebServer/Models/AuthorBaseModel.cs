using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AuthorBaseModel
{
	[JsonPropertyOrder(-4)]
	public string Id { get; set; }

	[JsonPropertyOrder(-3), Obsolete("Use Name property instead")]
	public string Nickname { get; set; }

	[JsonPropertyOrder(-2)]
	public string Name { get; set; }

	[JsonPropertyOrder(-1)]
	public string Title { get; set; }

	public AuthorBaseModel()
	{
	}

	public AuthorBaseModel(Author author)
	{
		Id = author.Id.ToString();
		Nickname = author.Name;
		Name = author.Name;
		Title = author.Title;
	}
}
