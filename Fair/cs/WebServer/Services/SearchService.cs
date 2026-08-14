using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SearchService
(
	ILogger<SearchService> logger,
	FairMcv mcv
)
{
	public IEnumerable<PublicationExtendedModel> SearchPublications([NotNull, NotEmpty] string storeId, [NotNull, NotEmpty] string query, string[]? categoriesIds, ProductType? type, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Query}, {CategoriesIds}, {Type}, {Page}, {PageSize}",
			nameof(SearchService), nameof(SearchService.SearchPublications), storeId, query, categoriesIds, type, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId[] effectiveCategoriesIds = GetEffectiveCategories(storeId, categoriesIds, type);

		HashSet<AutoId> allStoreCategories = new HashSet<AutoId>(effectiveCategoriesIds.Length * 4);
		GetAllNestedFilteredCategoriesNotOptimized(allStoreCategories, effectiveCategoriesIds, ProductType.None, type, cancellationToken);

		List<PublicationSearchResult> searchResults = mcv.PublicationTitles.Search(query, allStoreCategories, page * pageSize, pageSize);
		return searchResults.Count != 0 ? LoadPublications(searchResults, type, cancellationToken) : [];
	}

	AutoId[] GetEffectiveCategories(string storeId, string[]? categoriesIds, ProductType? productType)
	{
		if(categoriesIds.Length != 0)
		{
			return categoriesIds.Select(AutoId.Parse).ToArray();
		}

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return store.Categories;
	}

	IEnumerable<PublicationExtendedModel> LoadPublications(List<PublicationSearchResult> searchResult, ProductType? typeToSearch, CancellationToken cancellationToken)
	{
		foreach(var search in searchResult)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;

			Publication publication = mcv.Publications.Latest(search.Publication);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			ProductType? type = typeToSearch == null ? GetTypeFromCategory(category) : typeToSearch;

			yield return new PublicationExtendedModel(publication, product, author, category)
			{
				Type = type,
			};
		}
	}

	ProductType? GetTypeFromCategory(Category category)
	{
		if(category.Type != ProductType.None)
		{
			return category.Type;
		}

		while(category.Parent != null)
		{
			category = mcv.Categories.Latest(category.Parent);
			if(category.Type != ProductType.None)
			{
				return category.Type;
			}
		}

		return ProductType.None;
	}

	public IEnumerable<PublicationBaseModel> SearchLitePublications(string storeId, string query, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchLitePublications)} method called with {{StoreId}}, {{Query}}, {{Page}}, {{PageSize}}", storeId, query, page, pageSize);

		if(cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<PublicationBaseModel>();

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		HashSet<AutoId> allStoreCategories = new HashSet<AutoId>(store.Categories.Length * 4);
		GetAllNestedCategoriesNotOptimized(allStoreCategories, store.Categories, cancellationToken);

		List<PublicationSearchResult> searchResults = mcv.PublicationTitles.Search(query, allStoreCategories, page * pageSize, pageSize);
		return searchResults.Count != 0 ? LoadPublicationsBase(searchResults, cancellationToken) : [];
	}

	IEnumerable<PublicationBaseModel> LoadPublicationsBase(List<PublicationSearchResult> result, CancellationToken cancellationToken)
	{
		foreach(var item in result)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;

			Publication publication = mcv.Publications.Latest(item.Publication);
			Product product = mcv.Products.Latest(publication.Product);
			yield return new PublicationBaseModel(publication, product);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void GetAllNestedFilteredCategoriesNotOptimized(HashSet<AutoId> result, IEnumerable<AutoId> categoriesIds, ProductType inheritedType, ProductType? searchCategoryType, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return;

		foreach(var id in categoriesIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return;

			Category category = mcv.Categories.Latest(id);
			if(category == null)
			{
				continue;
			}

			ProductType effectiveType = category.Type != ProductType.None ? category.Type : inheritedType;

			if(category.Categories.Length > 0)
			{
				GetAllNestedFilteredCategoriesNotOptimized(result, category.Categories, effectiveType, searchCategoryType, cancellationToken);
			}
			if(searchCategoryType == null || effectiveType == searchCategoryType)
			{
				result.Add(id);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void GetAllNestedCategoriesNotOptimized(HashSet<AutoId> result, AutoId[] categoriesIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return;

		//result.Capacity += categoriesIds.Length;

		foreach(var i in categoriesIds)
			result.Add(i);

		foreach(var id in categoriesIds)
		{
			Category category = mcv.Categories.Latest(id);
			if(category != null && category.Categories.Length > 0)
			{
				GetAllNestedCategoriesNotOptimized(result, category.Categories, cancellationToken);
			}
		}
	}

	public TotalItemsResult<StoreBaseModel> SearchStores(string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchStores)} method called with {{Query}}, {{Page}}, {{PageSize}}", query, page, pageSize);

		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<StoreBaseModel>.Empty;

		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		var searchResult = mcv.StoreTitles.Search(query ?? "", page * pageSize, pageSize);
		if(searchResult.Length == 0)
		{
			return TotalItemsResult<StoreBaseModel>.Empty;
		}

		List<StoreBaseModel> result = new List<StoreBaseModel>(searchResult.Length);
		LoadStores(searchResult, result, cancellationToken);

		return new TotalItemsResult<StoreBaseModel>
		{
			Items = result,
			TotalItems = BitConverter.ToInt32(mcv.Metas.Latest(new MetaId((int)FairMetaEntityType.StoreCount)).Value)
		};
	}

	void LoadStores(StoreSearchResult[] searchResult, IList<StoreBaseModel> result, CancellationToken cancellationToken)
	{
		foreach(var search in searchResult)
		{
			if(cancellationToken.IsCancellationRequested)
				break;

			Store store = mcv.Stores.Latest(search.Entity);
			var model = new StoreBaseModel(store);
			result.Add(model);
		}
	}

	public IEnumerable<StoreSearchLiteModel> SearchLiteStores([NotEmpty, NotNull] string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchLiteStores)} method called with {{Query}}, {{Page}}, {{PageSize}}", query, page, pageSize);

		if(cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<StoreSearchLiteModel>();

		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		var result = mcv.StoreTitles.Search(query, page * pageSize, pageSize);

		return result.Select(x => new StoreSearchLiteModel(x.Entity.ToString(), x.Text));
	}

	public IEnumerable<UserModel> SearchStoreUsers([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Query}, {Limit}", nameof(SearchService), nameof(SearchStoreUsers), storeId, query, limit);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.NegativeOrZero(limit);

		AutoId storeEntityId = AutoId.Parse(storeId);

		if(AutoId.TryParse(query, out AutoId entityId))
		{
			FairUser user = (FairUser)mcv.Users.Latest(entityId);
			return user != null && user.Stores.Contains(storeEntityId) ? [new UserModel(user)] : [];
		}

		string lowercase = query.ToLower();
		IEnumerable<AutoId> searchResult = mcv.Names.Search(EntityTextField.UserName, lowercase, limit);

		return LoadUsers(storeEntityId, searchResult, cancellationToken);
	}

	IEnumerable<UserModel> LoadUsers(AutoId storeId, IEnumerable<AutoId> usersIds, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var result = new List<UserModel>(usersIds.Count());

		foreach(AutoId id in usersIds)
		{
			cancellationToken.ThrowIfCancellationRequested();

			FairUser user = (FairUser)mcv.Users.Latest(id);

			if(user.Stores.Contains(storeId))
			{
				var model = new UserModel(user);
				result.Add(model);
			}
		}

		return result;
	}

	public IEnumerable<UserBaseAvatarModel> SearchUser([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Limit}", nameof(SearchService), nameof(SearchUser), query, limit);

		if(cancellationToken.IsCancellationRequested)
			return [];

		Guard.Against.NullOrEmpty(query);

		if(AutoId.TryParse(query, out AutoId entityId))
		{
			FairUser account = (FairUser)mcv.Users.Latest(entityId);
			return [new UserBaseAvatarModel(account)];
		}

		string lowercase = query.ToLower();

		IEnumerable<AutoId> searchResult = mcv.Names.Search(EntityTextField.UserName, lowercase, limit);
		AutoId[] accountsIds = searchResult.ToArray();

		return LoadUsers(mcv, accountsIds, cancellationToken);
	}

	static IEnumerable<UserBaseAvatarModel> LoadUsers(Mcv mcv, AutoId[] accountsIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<UserBaseAvatarModel> result = new(accountsIds.Length);

		foreach(AutoId id in accountsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			FairUser account = (FairUser)mcv.Users.Latest(id);
			UserBaseAvatarModel model = new(account);
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

		IEnumerable<AutoId> searchResult = mcv.Names.Search(EntityTextField.AuthorName, lowercase, limit);
		AutoId[] authorsIds = searchResult.ToArray();

		return McvUtils.LoadAuthors(mcv, authorsIds, cancellationToken);
	}
}