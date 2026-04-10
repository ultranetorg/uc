using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class UserModel
{
	[JsonPropertyOrder(-3)]
	public string Id { get; init; }

	[JsonPropertyOrder(-2)]
	public string Name { get; init; }

	[JsonPropertyOrder(-1)]
	public string Owner { get; init; }
}
