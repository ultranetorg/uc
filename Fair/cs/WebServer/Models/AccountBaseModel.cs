using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AccountBaseModel(FairAccount account)
{
	[JsonPropertyOrder(-4)]
	public string Id { get; set; } = account.Id.ToString();

	[JsonPropertyOrder(-3)]
	public string Nickname { get; set; } = account.Nickname;

	[JsonPropertyOrder(-2)]
	public string Address { get; set; } = account.Address.ToString();

	[JsonPropertyOrder(-1)]
	public byte[]? Avatar { get; set; } = account.Avatar;
}
