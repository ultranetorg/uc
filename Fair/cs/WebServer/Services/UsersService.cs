using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersService
(
	ILogger<UsersService> logger,
	FairMcv mcv
)
{
	public TotalItemsResult<UserModel> GetStoreUsers([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(UsersService), nameof(GetStoreUsers), storeId, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

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

	public UserModel GetUserByName([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetUserByName), name);

		Guard.Against.NullOrEmpty(name);

		FairUser user = (FairUser) mcv.Users.Latest(name);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), name);
		}

		return GetUser(user);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	UserModel GetUser(FairUser account)
	{
		return new UserModel
		{
			Id = account.Id.ToString(),
			Name = account.Name,
			Owner = account.Owner.ToString()
		};
	}

	public UserDetailsModel GetDetails([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetDetails), name);

		Guard.Against.NullOrEmpty(name);

		FairUser account = (FairUser) mcv.Users.Latest(name);
		if(account == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), name);
		}

		return new UserDetailsModel
		{
			Id = account.Id.ToString(),
			Name = account.Name,
			Owner = account.Owner.ToString(),
			AuthorsIds = account.Authors.Select(id => id.ToString()),
			FavoriteStores = account.FavoriteStores.Length > 0 ? LoadUserStores(account.FavoriteStores) : [],
			HasAvatar = account.Avatar != null
		};
	}

	IEnumerable<StoreBaseModel> LoadUserStores(AutoId[] storesIds)
	{
		return storesIds.Select(id =>
		{
			Store store = mcv.Stores.Latest(id);
			return new StoreBaseModel(store);
		}).ToArray();
	}

	public UserAuthorsModel GetUserAuthors([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(GetUserAuthors), userId);

		Guard.Against.NullOrEmpty(userId);

		AutoId userEntityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		return new UserAuthorsModel
		{
			Id = user.Id.ToString(),
			Name = user.Name,
			Owner = user.Owner.ToString(),
			Authors = user.Authors.Length != 0 ? LoadAuthors(user.Authors) : []
		};
	}

	IEnumerable<AuthorBaseAvatarModel> LoadAuthors(AutoId[] authorsIds)
	{
		return authorsIds.Select(id =>
		{
			Author author = mcv.Authors.Latest(id);
			return new AuthorBaseAvatarModel(author);
		}).ToArray();
	}

	public bool StoreExists([NotNull][NotEmpty] string userId, [NotNull][NotEmpty] string storeId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}, {StoreId}", nameof(UsersService), nameof(StoreExists), userId, storeId);

		Guard.Against.NullOrEmpty(userId);
		Guard.Against.NullOrEmpty(storeId);

		AutoId userEntityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		AutoId storeEntityId = AutoId.Parse(storeId);
		return user.Stores.Contains(storeEntityId);
	}

	public FileContentResult GetAvatarById([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(GetAvatarById), userId);

		Guard.Against.NullOrEmpty(userId);

		AutoId entityId = AutoId.Parse(userId);

		FairUser account = (FairUser) mcv.Users.Latest(entityId);
		if(account == null || account.Avatar == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), userId);
		}

		return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
	}

	public FileContentResult GetAvatarByName([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetAvatarById), name);

		Guard.Against.NullOrEmpty(name);

		FairUser account = (FairUser)mcv.Users.Latest(name);
		if(account == null || account.Avatar == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), name);
		}

		return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
	}
}
