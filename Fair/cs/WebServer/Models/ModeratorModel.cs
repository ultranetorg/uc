namespace Uccs.Fair;

public class ModeratorModel(FairUser user, Time bannedTill)
{
	public AccountBaseModel User { get; } = new (user);

	public int BannedTill { get; } = bannedTill.Hours;
}
