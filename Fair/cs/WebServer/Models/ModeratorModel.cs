namespace Uccs.Fair;

public class ModeratorModel(FairUser user, Time bannedTill)
{
	public UserBaseModel User { get; } = new (user);

	public int BannedTill { get; } = bannedTill.Hours;
}
