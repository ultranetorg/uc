using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class AccountBaseAvatarModel(FairUser account) : AccountBaseModel(account)
{
	[JsonPropertyOrder(-1)]
	public byte[]? Avatar { get; } = account.Avatar;
}
