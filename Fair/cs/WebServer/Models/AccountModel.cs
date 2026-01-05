namespace Uccs.Fair;

public class AccountModel(FairUser account) : AccountBaseModel(account)
{
	public SiteBaseModel[] FavoriteSites { get; init; }
}
