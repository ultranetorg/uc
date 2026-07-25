using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class StoresService
(
	ILogger<StoresService> logger,
	FairMcv mcv
)
{
	public IEnumerable<StoreBaseModel> GetDefaultStores(CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called", nameof(StoresService), nameof(StoresService.GetDefaultStores));

		if (cancellationToken.IsCancellationRequested)
			return [];

		var result = new List<StoreBaseModel>(StoreConstants.DefaultStoresIds.Length);
		return LoadStores(StoreConstants.DefaultStoresIds, result, cancellationToken);
	}

	IList<StoreBaseModel> LoadStores(AutoId[] storesIds, IList<StoreBaseModel> result, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return result;

		foreach (AutoId storeId in storesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return result;

			Store store = mcv.Stores.Latest(storeId);
			if (store == null)
			{
				continue;
			}

			var model = new StoreBaseModel(store);
			result.Add(model);
		}

		return result;
	}

	public StoreModel GetDetails([NotEmpty] string storeId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(StoresService), nameof(GetDetails), storeId);

		Guard.Against.NullOrEmpty(storeId);

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		IEnumerable<string> moderatorsIds = store.Moderators.Where(x => x.BannedTill.Days == 0).Select(x => x.User.ToString());
		IEnumerable<string> authorsIds = store.Publishers.Where(x => x.BannedTill.Days == 0).Select(x => x.Author.ToString());

		return new StoreModel(store)
		{
			ModeratorsIds = moderatorsIds,
			AuthorsIds = authorsIds,
		};
	}

	public TotalItemsResult<PublisherModel> GetPublishers([NotEmpty][NotNull] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}", nameof(StoresService), nameof(GetPublishers), storeId, page, pageSize, search);

		Guard.Against.NullOrEmpty(storeId);

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		if(!string.IsNullOrEmpty(search) && AutoId.TryParse(search, out AutoId parsedId))
		{
			Publisher publisher = store.Publishers.FirstOrDefault(x => x.Author == parsedId);
			if(publisher == null)
			{
				return TotalItemsResult<PublisherModel>.Empty;
			}

			Author author = mcv.Authors.Latest(publisher.Author);
			var model = new PublisherModel(author, publisher);
			return new TotalItemsResult<PublisherModel> {Items = [model], TotalItems = store.Publishers.Length};
		}

		return LoadPublishers(store.Publishers, page, pageSize, search, cancellationToken);
	}

	TotalItemsResult<PublisherModel> LoadPublishers(IEnumerable<Publisher> publishers, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<PublisherModel>.Empty;

		List<PublisherModel> items = new(pageSize);
		int totalItems = 0;

		foreach(Publisher publisher in publishers)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<PublisherModel> { Items = items, TotalItems = totalItems };

			Author author = mcv.Authors.Latest(publisher.Author);
			if (!SearchUtils.IsMatch(author, search))
				continue;

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				var model = new PublisherModel(author, publisher);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<PublisherModel> { Items = items, TotalItems = totalItems };
	}

	public IEnumerable<ModeratorModel> GetModerators([NotEmpty] string storeId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(StoresService), nameof(GetModerators), storeId);

		Guard.Against.NullOrEmpty(storeId);

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadModerators(store.Moderators, cancellationToken);
	}

	public IEnumerable<ModeratorModel> LoadModerators(Moderator[] moderators, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<ModeratorModel> result = new(moderators.Length);

		foreach(Moderator moderator in moderators)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			FairUser account = (FairUser) mcv.Users.Latest(moderator.User);
			var model = new ModeratorModel(account, moderator.BannedTill);
			result.Add(model);
		}

		return result;
	}

	public IEnumerable<PolicyModel> GetPolicies(string storeId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(StoresService), nameof(GetPolicies), storeId);

		Guard.Against.NullOrEmpty(storeId);

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return store.Policies.Select(x => new PolicyModel(x));
	}
}
