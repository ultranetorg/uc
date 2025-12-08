namespace Uccs.Fair;

public class AccountModel(FairAccount account) : AccountBaseModel(account)
{
	public SiteBaseModel[] FavoriteSites { get; init; }
}
