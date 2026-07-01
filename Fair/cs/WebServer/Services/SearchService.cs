using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SearchService
(
	ILogger<SearchService> logger,
	FairMcv mcv
)
{
	public IEnumerable<PublicationExtendedModel> SearchPublications([NotNull, NotEmpty] string siteId, [NotNull, NotEmpty] string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Query}, {Page}, {PageSize}", nameof(SearchService), nameof(SearchService.SearchPublications), siteId, query, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		Site site = mcv.Sites.Latest(id);
		if(site == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		List<ProductSearchResult> searchResult = mcv.ProductTitles.Search(id, query, page * pageSize, pageSize);
		return searchResult.Count != 0 ? LoadPublications(searchResult, cancellationToken) : [];
	}

	IEnumerable<PublicationExtendedModel> LoadPublications(List<ProductSearchResult> searchResult, CancellationToken cancellationToken)
	{
		foreach(var search in searchResult)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;

			Publication publication = mcv.Publications.Latest(search.Publications[0]);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);

			yield return new PublicationExtendedModel(publication, product, author, category);
		}
	}

	public IEnumerable<PublicationBaseModel> SearchLitePublications(string siteId, string query, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchLitePublications)} method called with {{SiteId}}, {{Query}}, {{Page}}, {{PageSize}}", siteId, query, page, pageSize);

		if(cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<PublicationBaseModel>();

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		List<ProductSearchResult> result = mcv.ProductTitles.Search(id, query, page * pageSize, pageSize);
		return LoadPublicationsBase(result, cancellationToken);
	}

	IEnumerable<PublicationBaseModel> LoadPublicationsBase(List<ProductSearchResult> result, CancellationToken cancellationToken)
	{
		foreach(var item in result)
		{
			if (cancellationToken.IsCancellationRequested)
				yield break;

			Publication publication = mcv.Publications.Latest(item.Publications[0]);
			Product product = mcv.Products.Latest(item.Product);
			yield return new PublicationBaseModel(publication, product);
		}
	}

	public TotalItemsResult<SiteBaseModel> SearchSites(string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchSites)} method called with {{Query}}, {{Page}}, {{PageSize}}", query, page, pageSize);

		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<SiteBaseModel>.Empty;

		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		var searchResult = mcv.SiteTitles.Search(query ?? "", page * pageSize, pageSize);
		if(searchResult.Length == 0)
		{
			return TotalItemsResult<SiteBaseModel>.Empty;
		}

		List<SiteBaseModel> result = new List<SiteBaseModel>(searchResult.Length);
		LoadSites(searchResult, result, cancellationToken);

		return new TotalItemsResult<SiteBaseModel>
		{
			Items = result,
			TotalItems = BitConverter.ToInt32(mcv.Metas.Latest(new MetaId((int)FairMetaEntityType.SitesCount)).Value)
		};
	}

	void LoadSites(SiteSearchResult[] searchResult, IList<SiteBaseModel> result, CancellationToken cancellationToken)
	{
		foreach(var search in searchResult)
		{
			if(cancellationToken.IsCancellationRequested)
				break;

			Site site = mcv.Sites.Latest(search.Entity);
			var model = new SiteBaseModel(site);
			result.Add(model);
		}
	}

	public IEnumerable<SiteSearchLiteModel> SearchLiteSites([NotEmpty, NotNull] string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchLiteSites)} method called with {{Query}}, {{Page}}, {{PageSize}}", query, page, pageSize);

		if(cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<SiteSearchLiteModel>();

		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		var result = mcv.SiteTitles.Search(query, page * pageSize, pageSize);

		return result.Select(x => new SiteSearchLiteModel(x.Entity.ToString(), x.Text));
	}

	public IEnumerable<UserModel> SearchSiteUsers([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Query}, {Limit}", nameof(SearchService), nameof(SearchSiteUsers), siteId, query, limit);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.NegativeOrZero(limit);

		AutoId siteEntityId = AutoId.Parse(siteId);

		if(AutoId.TryParse(query, out AutoId entityId))
		{
			FairUser user = (FairUser)mcv.Users.Latest(entityId);
			return user != null && user.Sites.Contains(siteEntityId) ? [new UserModel(user)] : [];
		}

		string lowercase = query.ToLower();
		IEnumerable<AutoId> searchResult = mcv.Words.Search(EntityTextField.UserName, lowercase, limit);

		return LoadUsers(siteEntityId, searchResult, cancellationToken);
	}

	IEnumerable<UserModel> LoadUsers(AutoId siteId, IEnumerable<AutoId> usersIds, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var result = new List<UserModel>(usersIds.Count());

		foreach(AutoId id in usersIds)
		{
			cancellationToken.ThrowIfCancellationRequested();

			FairUser user = (FairUser)mcv.Users.Latest(id);

			if(user.Sites.Contains(siteId))
			{
				var model = new UserModel(user);
				result.Add(model);
			}
		}

		return result;
	}

	public IEnumerable<AccountBaseAvatarModel> SearchAccount([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Limit}", nameof(SearchService), nameof(SearchAccount), query, limit);

		if(cancellationToken.IsCancellationRequested)
			return [];

		Guard.Against.NullOrEmpty(query);

		if(AutoId.TryParse(query, out AutoId entityId))
		{
			FairUser account = (FairUser)mcv.Users.Latest(entityId);
			return [new AccountBaseAvatarModel(account)];
		}

		string lowercase = query.ToLower();

		IEnumerable<AutoId> searchResult = mcv.Words.Search(EntityTextField.UserName, lowercase, limit);
		AutoId[] accountsIds = searchResult.ToArray();

		return LoadAccounts(mcv, accountsIds, cancellationToken);
	}

	static IEnumerable<AccountBaseAvatarModel> LoadAccounts(Mcv mcv, AutoId[] accountsIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<AccountBaseAvatarModel> result = new(accountsIds.Length);

		foreach(AutoId id in accountsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			FairUser account = (FairUser)mcv.Users.Latest(id);
			AccountBaseAvatarModel model = new(account);
			result.Add(model);
		}

		return result;
	}

	public IEnumerable<AuthorBaseAvatarModel> SearchAuthors([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Limit}", nameof(SearchService), nameof(SearchAuthors), query, limit);

		if(cancellationToken.IsCancellationRequested)
			return [];

		Guard.Against.NullOrEmpty(query);
		Guard.Against.NegativeOrZero(limit);

		if(AutoId.TryParse(query, out AutoId entityId))
		{
			Author author = (Author)mcv.Authors.Latest(entityId);
			if(author != null)
			{
				return [new AuthorBaseAvatarModel(author)];
			}
		}

		string lowercase = query.ToLower();

		IEnumerable<AutoId> searchResult = mcv.Words.Search(EntityTextField.AuthorName, lowercase, limit);
		AutoId[] authorsIds = searchResult.ToArray();

		return McvUtils.LoadAuthors(mcv, authorsIds, cancellationToken);
	}

	public IEnumerable<AccountSearchLiteModel> SearchLiteAccounts(string query, int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Limit}", nameof(SearchService), nameof(SearchService.SearchLiteAccounts), query, limit);

		if(cancellationToken.IsCancellationRequested)
			return [];

		Guard.Against.NullOrEmpty(query);

		string lowercase = query.ToLower();

		IEnumerable<AutoId> searchResult = mcv.Words.Search(EntityTextField.UserName, lowercase, limit);

		var result = new List<AccountSearchLiteModel>(limit);
		return LoadAccounts(searchResult, result, cancellationToken);
	}

	IList<AccountSearchLiteModel> LoadAccounts(IEnumerable<AutoId> accounts, IList<AccountSearchLiteModel> result, CancellationToken cancellationToken)
	{
		foreach(AutoId accountId in accounts)
		{
			if(cancellationToken.IsCancellationRequested)
				break;

			FairUser account = (FairUser)mcv.Users.Latest(accountId);
			AccountSearchLiteModel model = new AccountSearchLiteModel
			{
				Id = account.Id.ToString(),
				Nickname = account.Name
			};
			result.Add(model);
		}

		return result;
	}
}