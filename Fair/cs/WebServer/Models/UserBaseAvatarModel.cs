using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class UserBaseAvatarModel(FairUser account) : UserBaseModel(account)
{
	[JsonPropertyOrder(-1)]
	public byte[]? Avatar { get; } = account.Avatar;
}
