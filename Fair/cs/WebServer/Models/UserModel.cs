using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class UserModel
{
	[JsonPropertyOrder(-3)]
	public AutoId Id { get; init; }

	[JsonPropertyOrder(-2)]
	public string Name { get; init; }

	[JsonPropertyOrder(-1)]
	public string Owner { get; init; }

	public UserModel()
	{
	}

	public UserModel(User user)
	{
		Id = user.Id;
		Name = user.Name;
		Owner = user.Key.ToString();
	}
}
