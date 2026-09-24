using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class UserBaseModel(FairUser account)
{
	[JsonPropertyOrder(-4)]
	public AutoId Id { get; } = account.Id;

	[JsonPropertyOrder(-3)]
	public string Nickname { get; } = account.Name;

	[JsonPropertyOrder(-2)]
	public string Address { get; } = account.Key.ToString();
}
