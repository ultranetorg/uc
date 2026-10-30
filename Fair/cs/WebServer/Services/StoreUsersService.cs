using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class StoreUsersService
(
#if DEBUG
	ILogger<StoreUsersService> logger,
#endif
	FairMcv mcv
)
{
	public TotalItemsResult<UserModel> GetStoreUsers([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(StoreUsersService), nameof(StoreUsersService.GetStoreUsers), storeId, page, pageSize);
#endif

		AutoId entityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(entityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store), storeId);
		}

		IEnumerable<AutoId> paged = store.Users.Skip(page * pageSize).Take(pageSize);
		IEnumerable<UserModel> items = store.Users.Length > 0 ? LoadUsers(paged, cancellationToken) : [];

		return new TotalItemsResult<UserModel>
		{
			Items = items,
			TotalItems = store.Users.Length
		};
	}

	IEnumerable<UserModel> LoadUsers(IEnumerable<AutoId> usersIds, CancellationToken cancellationToken)
	{
		return usersIds.Select(id =>
		{
			cancellationToken.ThrowIfCancellationRequested();

			FairUser user = (FairUser) mcv.Users.Latest(id);
			return new UserModel(user);
		}).ToArray();
	}

	public IEnumerable<UserModel> SearchStoreUsers([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentException.ThrowIfNullOrEmpty(query);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Query}, {Limit}", nameof(StoreUsersService), nameof(StoreUsersService.SearchStoreUsers), storeId, query, limit);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);

		if(AutoId.TryParse(query, out AutoId entityId))
		{
			FairUser user = (FairUser)mcv.Users.Latest(entityId);
			return user != null && user.Stores.Contains(storeEntityId) ? [new UserModel(user)] : [];
		}

		string lowercase = query.ToLower();
		IEnumerable<AutoId> searchResult = mcv.Names.Search(EntityTextField.UserName, lowercase, limit);

		return LoadStoreUsers(storeEntityId, searchResult, cancellationToken);
	}

	IEnumerable<UserModel> LoadStoreUsers(AutoId storeId, IEnumerable<AutoId> usersIds, CancellationToken cancellationToken)
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
}
