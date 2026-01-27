namespace Uccs.Fair;

public class AccountModel(FairUser account) : AccountBaseModel(account)
{
	public SiteBaseModel[] FavoriteSites { get; init; }

	public IEnumerable<string> AuthorsIds { get; init; } = account.Authors.Select(x => x.ToString());
}
