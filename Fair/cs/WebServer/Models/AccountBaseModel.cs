using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AccountBaseModel
{
	[JsonPropertyOrder(-3)]
	public string Id { get; set; }

	[JsonPropertyOrder(-2)]
	public string Nickname { get; set; }

	[JsonPropertyOrder(-1)]
	public string Address { get; set; }

	public AccountBaseModel(FairAccount account)
	{
		Id = account.Id.ToString();
		Nickname = account.Nickname;
		Address = account.Address.ToString();
	}
}
