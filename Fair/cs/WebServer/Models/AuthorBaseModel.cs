using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AuthorBaseModel
{
	[JsonPropertyOrder(-5)]
	public string Id { get; set; }

	[JsonPropertyOrder(-4)]
	public string Title { get; set; }

	[JsonPropertyOrder(-3)]
	public int Expiration { get; set; }

	[JsonPropertyOrder(-2)]
	public int SpaceReserved { get; set; }
	[JsonPropertyOrder(-1)]
	public int SpaceUsed { get; set; }

	public AuthorBaseModel(Author author)
	{
		Id = author.Id.ToString();
		Title = author.Title;
		Expiration = author.Expiration.Days;
		SpaceReserved = author.SpaceReserved;
		SpaceUsed = author.SpaceUsed;
	}
}
