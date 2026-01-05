using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AccountBaseModel(FairUser account)
{
	[JsonPropertyOrder(-4)]
	public string Id { get; } = account.Id.ToString();

	[JsonPropertyOrder(-3)]
	public string Nickname { get; } = account.Name;

	[JsonPropertyOrder(-2)]
	public string Address { get; } = account.Owner.ToString();
}
