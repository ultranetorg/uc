using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class StoresService
(
#if DEBUG
	ILogger<StoresService> logger,
#endif
	FairMcv mcv
)
{
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
		ArgumentException.ThrowIfNullOrEmpty(storeId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(StoresService), nameof(StoresService.GetDetails), storeId);
#endif

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		IEnumerable<AutoId> moderatorsIds = store.Moderators.Where(x => x.BannedTill == Time.Zero).Select(x => x.User);
		IEnumerable<AutoId> authorsIds = store.Publishers.Where(x => x.BannedTill == Time.Zero).Select(x => x.Author);

		return new StoreModel(store)
		{
			ModeratorsIds = moderatorsIds,
			AuthorsIds = authorsIds,
			HasPublications = store.PublicationsCount > 0
		};
	}

	public IEnumerable<PolicyModel> GetPolicies(string storeId)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(StoresService), nameof(StoresService.GetPolicies), storeId);
#endif

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return store.Policies.Select(x => new PolicyModel(x));
	}
}
